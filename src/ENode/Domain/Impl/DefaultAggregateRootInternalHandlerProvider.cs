using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    /// <summary>The default implementation of IAggregateRootInternalHandlerProvider and IAssemblyInitializer.
    /// </summary>
    public class DefaultAggregateRootInternalHandlerProvider : IAggregateRootInternalHandlerProvider, IAssemblyInitializer
    {
        private readonly IDictionary<Type, IDictionary<Type, MethodInfo>> _mappings = new Dictionary<Type, IDictionary<Type, MethodInfo>>();

        /// <summary>Initialize from the given assemblies.
        /// </summary>
        /// <param name="assemblies"></param>
        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var aggregateRootType in assembly.GetTypes().Where(TypeUtils.IsAggregateRoot))
                {
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

        /// <summary>Get the internal event handler within the aggregate.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public Action<AggregateRoot, object> GetInternalEventHandler(Type aggregateRootType, Type eventType)
        {
            IDictionary<Type, MethodInfo> eventHandlerDic;
            if (!_mappings.TryGetValue(aggregateRootType, out eventHandlerDic)) return null;
            MethodInfo eventHandler;
            return eventHandlerDic.TryGetValue(eventType, out eventHandler) ? new Action<AggregateRoot, object>((aggregateRoot, evnt) => eventHandler.Invoke(aggregateRoot, new[] { evnt })) : null;
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

        private static Type GetEventType(Type eventHandlerInterface)
        {
            return eventHandlerInterface.GetGenericArguments().Single();
        }
        private static IEnumerable<Type> ScanEventHandlerInterfaces(Type eventHandlerType)
        {
            return eventHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        }
    }
}
