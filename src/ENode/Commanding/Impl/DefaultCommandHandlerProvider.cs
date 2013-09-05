using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandHandlerProvider.
    /// </summary>
    public class DefaultCommandHandlerProvider : ICommandHandlerProvider, IAssemblyInitializer
    {
        private readonly ConcurrentDictionary<Type, ICommandHandler> _commandHandlerDict = new ConcurrentDictionary<Type, ICommandHandler>();

        /// <summary>Initialize the provider with the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to initialize.</param>
        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAllCommandHandlersInAssembly(assembly);
            }
        }
        /// <summary>Get the command handler for the given command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public ICommandHandler GetCommandHandler(ICommand command)
        {
            ICommandHandler commandHandler;
            return _commandHandlerDict.TryGetValue(command.GetType(), out commandHandler) ? commandHandler : null;
        }
        /// <summary>Check whether the given type is a command handler type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsCommandHandler(Type type)
        {
            return type.IsInterface == false && type.IsAbstract == false && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
        }

        private void RegisterAllCommandHandlersInAssembly(Assembly assembly)
        {
            foreach (var commandHandlerType in assembly.GetTypes().Where(IsCommandHandler))
            {
                if (!TypeUtils.IsComponent(commandHandlerType))
                {
                    throw new Exception(string.Format("{0} should be marked as component.", commandHandlerType.FullName));
                }
                var handlerTypes = commandHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));

                foreach (var handlerType in handlerTypes)
                {
                    var commandType = handlerType.GetGenericArguments().Single();
                    var commandHandlerWrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(commandType);
                    var commandHandler = ObjectContainer.Resolve(commandHandlerType);
                    var commandHandlerWrapper = Activator.CreateInstance(commandHandlerWrapperType, new[] { commandHandler }) as ICommandHandler;
                    RegisterCommandHandler(commandType, commandHandlerWrapper);
                }
            }
        }
        private void RegisterCommandHandler(Type commandType, ICommandHandler commandHandler)
        {
            if (_commandHandlerDict.TryAdd(commandType, commandHandler)) return;

            if (_commandHandlerDict.ContainsKey(commandType))
            {
                throw new DuplicatedCommandHandlerException(commandType, commandHandler.GetInnerCommandHandler().GetType());
            }
            throw new ENodeException("Error occurred when registering {0} for {1} command.", commandHandler.GetType().Name, commandType.Name);
        }
    }
}
