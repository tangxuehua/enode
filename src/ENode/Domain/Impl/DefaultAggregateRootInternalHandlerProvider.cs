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
        private readonly IDictionary<Type, IDictionary<Type, Action<AggregateRoot, IEvent>>> _mappings = new Dictionary<Type, IDictionary<Type, Action<AggregateRoot, IEvent>>>();
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        private readonly Type[] parameterTypes = new Type[] { typeof(AggregateRoot), typeof(IEvent) };

        /// <summary>Initialize from the given assemblies.
        /// </summary>
        /// <param name="assemblies"></param>
        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var aggregateRootType in assembly.GetTypes().Where(TypeUtils.IsAggregateRoot))
                {
                    var entries = from method in aggregateRootType.GetMethods(bindingFlags)
                                  let parameters = method.GetParameters()
                                  where method.Name.ToUpper() == "HANDLE" && parameters.Length == 1 && typeof(IEvent).IsAssignableFrom(parameters.First().ParameterType)
                                  select new { Method = method, EventType = parameters.First().ParameterType };
                    foreach (var entry in entries)
                    {
                        RegisterInternalHandler(aggregateRootType, entry.EventType, entry.Method);
                    }
                }
            }
        }

        /// <summary>Get the internal event handler within the aggregate.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public Action<AggregateRoot, IEvent> GetInternalEventHandler(Type aggregateRootType, Type eventType)
        {
            IDictionary<Type, Action<AggregateRoot, IEvent>> eventHandlerDic;
            if (!_mappings.TryGetValue(aggregateRootType, out eventHandlerDic)) return null;
            Action<AggregateRoot, IEvent> eventHandler;
            return eventHandlerDic.TryGetValue(eventType, out eventHandler) ? eventHandler : null;
        }

        private void RegisterInternalHandler(Type aggregateRootType, Type eventType, MethodInfo eventHandler)
        {
            IDictionary<Type, Action<AggregateRoot, IEvent>> eventHandlerDic;

            if (!_mappings.TryGetValue(aggregateRootType, out eventHandlerDic))
            {
                eventHandlerDic = new Dictionary<Type, Action<AggregateRoot, IEvent>>();
                _mappings.Add(aggregateRootType, eventHandlerDic);
            }

            if (eventHandlerDic.ContainsKey(eventType))
            {
                throw new Exception(string.Format("Found duplicated event handler on aggregate. Aggregate type:{0}, event type:{1}", aggregateRootType.FullName, eventType.FullName));
            }
            eventHandlerDic.Add(eventType, DelegateFactory.CreateDelegate<Action<AggregateRoot, IEvent>>(eventHandler, parameterTypes));
        }
    }
}
