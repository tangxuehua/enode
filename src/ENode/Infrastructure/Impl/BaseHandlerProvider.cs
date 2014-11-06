using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;

namespace ENode.Infrastructure.Impl
{
    public abstract class BaseHandlerProvider<TMessageHandlerInterface> : IMessageHandlerProvider<TMessageHandlerInterface>, IAssemblyInitializer
        where TMessageHandlerInterface : class, IMessageHandler
    {
        private readonly IDictionary<Type, IList<TMessageHandlerInterface>> _messageHandlerDict = new Dictionary<Type, IList<TMessageHandlerInterface>>();

        protected abstract Type GetMessageHandlerGenericInterfaceType();
        protected abstract Type GetMessageHandlerWrapperType();

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var handlerType in assemblies.SelectMany(assembly => assembly.GetTypes().Where(IsMessageHandler)))
            {
                if (!TypeUtils.IsComponent(handlerType))
                {
                    throw new Exception(string.Format("Message handler [type={0}] should be marked as component.", handlerType.FullName));
                }
                RegisterMessageHandler(handlerType);
            }
        }
        public IEnumerable<TMessageHandlerInterface> GetMessageHandlers(Type type)
        {
            var messageHandlers = new List<TMessageHandlerInterface>();
            foreach (var key in _messageHandlerDict.Keys.Where(key => key.IsAssignableFrom(type)))
            {
                messageHandlers.AddRange(_messageHandlerDict[key]);
            }
            return messageHandlers;
        }
        public bool IsMessageHandler(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && ScanHandlerInterfaces(type).Any();
        }

        private void RegisterMessageHandler(Type messageHandlerType)
        {
            foreach (var handlerInterfaceType in ScanHandlerInterfaces(messageHandlerType))
            {
                var argumentType = handlerInterfaceType.GetGenericArguments().Single();
                var handlerWrapperType = GetMessageHandlerWrapperType().MakeGenericType(argumentType);
                IList<TMessageHandlerInterface> messageHandlers;
                if (!_messageHandlerDict.TryGetValue(argumentType, out messageHandlers))
                {
                    messageHandlers = new List<TMessageHandlerInterface>();
                    _messageHandlerDict.Add(argumentType, messageHandlers);
                }

                if (messageHandlers.Any(x => x.GetInnerHandler().GetType() == messageHandlerType)) continue;

                var messageHandler = ObjectContainer.Resolve(messageHandlerType);
                var messageHandlerWrapper = Activator.CreateInstance(handlerWrapperType, new[] { messageHandler }) as TMessageHandlerInterface;
                messageHandlers.Add(messageHandlerWrapper);
            }
        }
        private IEnumerable<Type> ScanHandlerInterfaces(Type type)
        {
            return type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == GetMessageHandlerGenericInterfaceType());
        }
    }
}
