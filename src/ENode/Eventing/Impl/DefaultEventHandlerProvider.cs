using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class DefaultEventHandlerProvider : IEventHandlerProvider, IAssemblyInitializer
    {
        private readonly IDictionary<Type, IList<IEventHandler>> _eventHandlerDict = new Dictionary<Type, IList<IEventHandler>>();

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var handlerType in assembly.GetTypes().Where(x => IsEventHandler(x)))
                {
                    if (!TypeUtils.IsComponent(handlerType))
                    {
                        throw new Exception(string.Format("{0} should be marked as component.", handlerType.FullName));
                    }
                    RegisterEventHandler(handlerType);
                }
            }
        }

        public IEnumerable<IEventHandler> GetEventHandlers(Type eventType)
        {
            var eventHandlers = new List<IEventHandler>();
            foreach (var key in _eventHandlerDict.Keys)
            {
                if (key.IsAssignableFrom(eventType))
                {
                    eventHandlers.AddRange(_eventHandlerDict[key]);
                }
            }
            return eventHandlers;
        }
        public bool IsEventHandler(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && ScanEventHandlerInterfaces(type).Count() > 0 && !typeof(AggregateRoot).IsAssignableFrom(type);
        }

        private void RegisterEventHandler(Type eventHandlerType)
        {
            foreach (var eventHandlerInterface in ScanEventHandlerInterfaces(eventHandlerType))
            {
                var eventType = GetEventType(eventHandlerInterface);
                var eventHandlerWrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
                IList<IEventHandler> eventHandlers = null;
                if (!_eventHandlerDict.TryGetValue(eventType, out eventHandlers))
                {
                    eventHandlers = new List<IEventHandler>();
                    _eventHandlerDict.Add(eventType, eventHandlers);
                }

                if (!eventHandlers.Any(x => x.GetInnerEventHandler().GetType() == eventHandlerType))
                {
                    var eventHandler = ObjectContainer.Resolve(eventHandlerType);
                    var eventHandlerWrapper = Activator.CreateInstance(eventHandlerWrapperType, new object[] { eventHandler }) as IEventHandler;
                    eventHandlers.Add(eventHandlerWrapper);
                }
            }
        }
        private IEnumerable<Type> ScanEventHandlerInterfaces(Type eventHandlerType)
        {
            return eventHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        }
        private Type GetEventType(Type eventHandlerInterface)
        {
            return eventHandlerInterface.GetGenericArguments().Single();
        }
    }
}
