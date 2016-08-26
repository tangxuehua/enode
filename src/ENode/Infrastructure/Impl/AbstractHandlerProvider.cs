using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;

namespace ENode.Infrastructure.Impl
{
    public abstract class AbstractHandlerProvider<TKey, THandlerProxyInterface, THandlerSource> : IAssemblyInitializer
        where THandlerProxyInterface : class, IObjectProxy
    {
        private readonly IDictionary<TKey, IList<THandlerProxyInterface>> _handlerDict = new Dictionary<TKey, IList<THandlerProxyInterface>>();
        private readonly IDictionary<TKey, MessageHandlerData<THandlerProxyInterface>> _messageHandlerDict = new Dictionary<TKey, MessageHandlerData<THandlerProxyInterface>>();

        protected abstract Type GetGenericHandlerType();
        protected abstract TKey GetKey(Type handlerInterfaceType);
        protected abstract Type GetHandlerProxyImplementationType(Type handlerInterfaceType);
        protected abstract bool IsHandlerSourceMatchKey(THandlerSource handlerSource, TKey key);
        protected abstract bool IsHandleMethodMatchKey(Type[] argumentTypes, TKey key);

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var handlerType in assemblies.SelectMany(assembly => assembly.GetTypes().Where(IsHandlerType)))
            {
                RegisterHandler(handlerType);
            }
            InitializeHandlerPriority();
        }
        public IEnumerable<MessageHandlerData<THandlerProxyInterface>> GetHandlers(THandlerSource source)
        {
            var handlerDataList = new List<MessageHandlerData<THandlerProxyInterface>>();
            foreach (var key in _messageHandlerDict.Keys.Where(key => IsHandlerSourceMatchKey(source, key)))
            {
                handlerDataList.Add(_messageHandlerDict[key]);
            }
            return handlerDataList;
        }

        private void InitializeHandlerPriority()
        {
            foreach (var entry in _handlerDict)
            {
                var key = entry.Key;
                var handlers = entry.Value;
                MessageHandlerData<THandlerProxyInterface> handlerData = new MessageHandlerData<THandlerProxyInterface>();
                var listHandlers = new List<THandlerProxyInterface>();
                var queueHandlerDict = new Dictionary<THandlerProxyInterface, int>();
                foreach (var handler in handlers)
                {
                    var priority = GetHandleMethodPriority(handler, key);
                    if (priority == null)
                    {
                        listHandlers.Add(handler);
                    }
                    else
                    {
                        queueHandlerDict.Add(handler, priority.Value);
                    }
                }
                handlerData.AllHandlers = handlers;
                handlerData.ListHandlers = listHandlers;
                handlerData.QueuedHandlers = queueHandlerDict.OrderBy(x => x.Value).Select(x => x.Key);
                _messageHandlerDict.Add(key, handlerData);
            }
        }
        private int? GetHandleMethodPriority(THandlerProxyInterface handler, TKey key)
        {
            var handleMethods = handler.GetInnerObject().GetType().GetMethods().Where(x => x.Name == "HandleAsync");
            foreach (var method in handleMethods)
            {
                var argumentTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
                if (IsHandleMethodMatchKey(argumentTypes, key))
                {
                    var methodPriorityAttributes = method.GetCustomAttributes(typeof(PriorityAttribute), false);
                    var classPriorityAttributes = handler.GetInnerObject().GetType().GetCustomAttributes(typeof(PriorityAttribute), false);
                    if (methodPriorityAttributes.Any())
                    {
                        return ((PriorityAttribute)methodPriorityAttributes.First()).Priority;
                    }
                    else if (classPriorityAttributes.Any())
                    {
                        return ((PriorityAttribute)classPriorityAttributes.First()).Priority;
                    }
                    break;
                }
            }
            return null;
        }
        private bool IsHandlerType(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && ScanHandlerInterfaces(type).Any();
        }
        private void RegisterHandler(Type handlerType)
        {
            var handlerInterfaceTypes = ScanHandlerInterfaces(handlerType);

            foreach (var handlerInterfaceType in handlerInterfaceTypes)
            {
                var key = GetKey(handlerInterfaceType);
                var handlerProxyType = GetHandlerProxyImplementationType(handlerInterfaceType);
                IList<THandlerProxyInterface> handlers;
                if (!_handlerDict.TryGetValue(key, out handlers))
                {
                    handlers = new List<THandlerProxyInterface>();
                    _handlerDict.Add(key, handlers);
                }

                var handler = handlers.SingleOrDefault(x => x.GetInnerObject().GetType() == handlerType);
                if (handler != null)
                {
                    throw new InvalidOperationException("Handler cannot handle duplicate message, handlerType:" + handlerType);
                }

                var lifeStyle = ParseComponentLife(handlerType);
                var realHandler = default(object);
                if (lifeStyle == LifeStyle.Singleton)
                {
                    realHandler = ObjectContainer.Resolve(handlerType);
                }
                handlers.Add(Activator.CreateInstance(handlerProxyType, new[] { realHandler, handlerType }) as THandlerProxyInterface);
            }
        }
        private IEnumerable<Type> ScanHandlerInterfaces(Type type)
        {
            return type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == GetGenericHandlerType());
        }
        private static LifeStyle ParseComponentLife(Type type)
        {
            var attributes = type.GetCustomAttributes<ComponentAttribute>(false);
            if (attributes != null && attributes.Any())
            {
                return attributes.First().LifeStyle;
            }
            return LifeStyle.Singleton;
        }
    }
}
