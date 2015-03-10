using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;

namespace ENode.Infrastructure.Impl
{
    public abstract class AbstractHandlerProvider<THandlerProxyInterface> : IAssemblyInitializer
        where THandlerProxyInterface : class, IHandlerProxy
    {
        private readonly IDictionary<Type, IList<THandlerProxyInterface>> _handlerDict = new Dictionary<Type, IList<THandlerProxyInterface>>();

        protected abstract Type GetGenericHandlerType();
        protected abstract Type GetHandlerProxyImplementationType();

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var handlerType in assemblies.SelectMany(assembly => assembly.GetTypes().Where(IsHandlerType)))
            {
                if (!TypeUtils.IsComponent(handlerType))
                {
                    throw new Exception(string.Format("Handler [type={0}] should be marked as component.", handlerType.FullName));
                }
                RegisterHandler(handlerType);
            }
        }
        public IEnumerable<THandlerProxyInterface> GetHandlers(Type messageType)
        {
            var handlers = new List<THandlerProxyInterface>();
            foreach (var key in _handlerDict.Keys.Where(key => key.IsAssignableFrom(messageType)))
            {
                handlers.AddRange(_handlerDict[key]);
            }
            return handlers;
        }

        private bool IsHandlerType(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && ScanHandlerInterfaces(type).Any();
        }
        private void RegisterHandler(Type handlerType)
        {
            foreach (var handlerInterfaceType in ScanHandlerInterfaces(handlerType))
            {
                var argumentType = handlerInterfaceType.GetGenericArguments().Single();
                var handlerProxyType = GetHandlerProxyImplementationType().MakeGenericType(argumentType);
                IList<THandlerProxyInterface> handlers;
                if (!_handlerDict.TryGetValue(argumentType, out handlers))
                {
                    handlers = new List<THandlerProxyInterface>();
                    _handlerDict.Add(argumentType, handlers);
                }

                if (handlers.Any(x => x.GetInnerHandler().GetType() == handlerType))
                {
                    continue;
                }

                handlers.Add(Activator.CreateInstance(handlerProxyType, new[] { ObjectContainer.Resolve(handlerType) }) as THandlerProxyInterface);
            }
        }
        private IEnumerable<Type> ScanHandlerInterfaces(Type type)
        {
            return type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == GetGenericHandlerType());
        }
    }
}
