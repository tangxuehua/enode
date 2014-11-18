using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;

namespace ENode.Infrastructure.Impl
{
    public abstract class BaseHandlerProvider<THandlerInterface> : IAssemblyInitializer where THandlerInterface : class
    {
        private readonly IDictionary<Type, IList<object>> _handlerDict = new Dictionary<Type, IList<object>>();

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var handlerType in assemblies.SelectMany(assembly => assembly.GetTypes().Where(IsHandler)))
            {
                if (!TypeUtils.IsComponent(handlerType))
                {
                    throw new Exception(string.Format("Handler [type={0}] should be marked as component.", handlerType.FullName));
                }
                RegisterHandler(handlerType);
            }
        }

        public IEnumerable<THandlerInterface> GetHandlers(Type type)
        {
            var handlers = new List<object>();
            foreach (var key in _handlerDict.Keys.Where(key => key.IsAssignableFrom(type)))
            {
                handlers.AddRange(_handlerDict[key]);
            }
            return handlers.Cast<THandlerInterface>();
        }
        public bool IsHandler(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && ScanHandlerInterfaces(type).Any();
        }

        protected abstract Type GetHandlerGenericInterfaceType();
        protected abstract Type GetHandlerProxyType();

        private void RegisterHandler(Type handlerType)
        {
            foreach (var handlerInterfaceType in ScanHandlerInterfaces(handlerType))
            {
                var argumentType = handlerInterfaceType.GetGenericArguments().Single();
                var handlerProxyType = GetHandlerProxyType().MakeGenericType(argumentType);
                IList<object> handlers;
                if (!_handlerDict.TryGetValue(argumentType, out handlers))
                {
                    handlers = new List<object>();
                    _handlerDict.Add(argumentType, handlers);
                }

                if (handlers.Any(x =>
                    {
                        var handlerProxy = x as IProxyHandler;
                        if (handlerProxy != null)
                        {
                            return handlerProxy.GetInnerHandler().GetType() == handlerType;
                        }
                        else
                        {
                            return x.GetType() == handlerType;
                        }
                    }))
                {
                    continue;
                }

                handlers.Add(Activator.CreateInstance(handlerProxyType, new[] { ObjectContainer.Resolve(handlerType) }));
            }
        }
        private IEnumerable<Type> ScanHandlerInterfaces(Type type)
        {
            return type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == GetHandlerGenericInterfaceType());
        }
    }
}
