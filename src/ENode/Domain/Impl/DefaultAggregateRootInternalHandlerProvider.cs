using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateRootInternalHandlerProvider : IAggregateRootInternalHandlerProvider, IAssemblyInitializer
    {
        private readonly IDictionary<Type, IDictionary<Type, Action<IAggregateRoot, IDomainEvent>>> _mappings = new Dictionary<Type, IDictionary<Type, Action<IAggregateRoot, IDomainEvent>>>();
        private readonly BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        private readonly Type[] parameterTypes = new Type[] { typeof(IAggregateRoot), typeof(IDomainEvent) };

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var aggregateRootType in assembly.GetTypes().Where(ENode.Infrastructure.TypeUtils.IsAggregateRoot))
                {
                    var entries = from method in aggregateRootType.GetMethods(bindingFlags)
                                  let parameters = method.GetParameters()
                                  where method.Name == "Handle"
                                      && parameters.Length == 1
                                      && typeof(IDomainEvent).IsAssignableFrom(parameters.Single().ParameterType)
                                  select new { Method = method, EventType = parameters.Single().ParameterType };
                    foreach (var entry in entries)
                    {
                        RegisterInternalHandler(aggregateRootType, entry.EventType, entry.Method);
                    }
                }
            }
        }
        public Action<IAggregateRoot, IDomainEvent> GetInternalEventHandler(Type aggregateRootType, Type eventType)
        {
            IDictionary<Type, Action<IAggregateRoot, IDomainEvent>> eventHandlerDic;
            if (!_mappings.TryGetValue(aggregateRootType, out eventHandlerDic)) return null;
            Action<IAggregateRoot, IDomainEvent> eventHandler;
            return eventHandlerDic.TryGetValue(eventType, out eventHandler) ? eventHandler : null;
        }

        private void RegisterInternalHandler(Type aggregateRootType, Type eventType, MethodInfo eventHandler)
        {
            IDictionary<Type, Action<IAggregateRoot, IDomainEvent>> eventHandlerDic;

            if (!_mappings.TryGetValue(aggregateRootType, out eventHandlerDic))
            {
                eventHandlerDic = new Dictionary<Type, Action<IAggregateRoot, IDomainEvent>>();
                _mappings.Add(aggregateRootType, eventHandlerDic);
            }

            if (eventHandlerDic.ContainsKey(eventType))
            {
                throw new Exception(string.Format("Found duplicated event handler on aggregate, aggregate type:{0}, event type:{1}", aggregateRootType.FullName, eventType.FullName));
            }
            eventHandlerDic.Add(eventType, DelegateFactory.CreateDelegate<Action<IAggregateRoot, IDomainEvent>>(eventHandler, parameterTypes));
        }
    }
}
