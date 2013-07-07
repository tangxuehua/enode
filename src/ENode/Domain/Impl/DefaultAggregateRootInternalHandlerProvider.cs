using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain
{
    public class DefaultAggregateRootInternalHandlerProvider : IAggregateRootInternalHandlerProvider, IAssemblyInitializer
    {
        private IDictionary<Type, IDictionary<Type, MethodInfo>> _mappings = new Dictionary<Type, IDictionary<Type, MethodInfo>>();

        public void Initialize(params Assembly[] assemblies)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            foreach (var assembly in assemblies)
            {
                foreach (var aggregateRootType in assembly.GetTypes().Where(t => TypeUtils.IsAggregateRoot(t)))
                {
                    var entries = from method in aggregateRootType.GetMethods(bindingFlags)
                                  let parameters = method.GetParameters()
                                  where (method.Name == "Handle" || method.Name.StartsWith("On")) && parameters.Length == 1
                                  select new { Method = method, EventType = parameters.First().ParameterType };

                    foreach (var eventHandlerInterface in ScanEventHandlerInterfaces(aggregateRootType))
                    {
                        var mapping = aggregateRootType.GetInterfaceMap(eventHandlerInterface);
                        var eventType = GetEventType(eventHandlerInterface);
                        var method = mapping.TargetMethods.Single();
                        RegisterInternalHandler(aggregateRootType, eventType, method);
                    }
                }
            }
        }

        public Action<AggregateRoot, object> GetInternalEventHandler(Type aggregateRootType, Type eventType)
        {
            IDictionary<Type, MethodInfo> eventHandlerDic;
            MethodInfo eventHandler;

            if (_mappings.TryGetValue(aggregateRootType, out eventHandlerDic))
            {
                if (eventHandlerDic.TryGetValue(eventType, out eventHandler))
                {
                    return new Action<AggregateRoot, object>((aggregateRoot, evnt) => eventHandler.Invoke(aggregateRoot, new object[] { evnt }));
                }
            }

            return null;
        }

        private void RegisterInternalHandler(Type aggregateRootType, Type eventType, MethodInfo eventHandler)
        {
            IDictionary<Type, MethodInfo> eventHandlerDic;

            if (!_mappings.TryGetValue(aggregateRootType, out eventHandlerDic))
            {
                eventHandlerDic = new Dictionary<Type, MethodInfo>();
                _mappings.Add(aggregateRootType, eventHandlerDic);
            }

            if (eventHandlerDic.ContainsKey(eventType))
            {
                throw new Exception(string.Format("Found duplicated event handler on aggregate. Aggregate type:{0}, event type:{1}", aggregateRootType.FullName, eventType.FullName));
            }

            eventHandlerDic.Add(eventType, eventHandler);
        }

        private Type GetEventType(Type eventHandlerInterface)
        {
            return eventHandlerInterface.GetGenericArguments().Single();
        }
        private IEnumerable<Type> ScanEventHandlerInterfaces(Type eventHandlerType)
        {
            return eventHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        }
    }
}
