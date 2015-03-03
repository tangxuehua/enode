using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;

namespace ENode.Infrastructure.Impl
{
    public class DefaultHandlerProvider : IHandlerProvider, IAssemblyInitializer
    {
        private readonly IDictionary<Type, IList<IProxyHandler>> _handlerDict = new Dictionary<Type, IList<IProxyHandler>>();

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
        public IEnumerable<IProxyHandler> GetHandlers(Type messageType)
        {
            var handlers = new List<IProxyHandler>();
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
                var handlerProxyType = typeof(DefaultProxyHandler<>).MakeGenericType(argumentType);
                IList<IProxyHandler> handlers;
                if (!_handlerDict.TryGetValue(argumentType, out handlers))
                {
                    handlers = new List<IProxyHandler>();
                    _handlerDict.Add(argumentType, handlers);
                }

                if (handlers.Any(x => x.GetInnerHandler().GetType() == handlerType))
                {
                    continue;
                }

                handlers.Add(Activator.CreateInstance(handlerProxyType, new[] { ObjectContainer.Resolve(handlerType) }) as IProxyHandler);
            }
        }
        private IEnumerable<Type> ScanHandlerInterfaces(Type type)
        {
            return type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandler<>));
        }
    }
}
