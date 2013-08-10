using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using ENode.Infrastructure;

namespace ENode.Commanding {
    public class DefaultCommandHandlerProvider : ICommandHandlerProvider, IAssemblyInitializer {
        private ConcurrentDictionary<Type, ICommandHandler> _commandHandlerDict = new ConcurrentDictionary<Type, ICommandHandler>();

        public void Initialize(params Assembly[] assemblies) {
            foreach (var assembly in assemblies) {
                RegisterAllCommandHandlersInAssembly(assembly);
            }
        }
        public ICommandHandler GetCommandHandler(ICommand command) {
            ICommandHandler commandHandler;
            if (_commandHandlerDict.TryGetValue(command.GetType(), out commandHandler)) {
                return commandHandler;
            }
            return null;
        }
        public bool IsCommandHandler(Type type) {
            return type.IsInterface == false && type.IsAbstract == false && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
        }

        private void RegisterAllCommandHandlersInAssembly(Assembly assembly) {
            foreach (var commandHandlerType in assembly.GetTypes().Where(x => IsCommandHandler(x))) {
                if (!TypeUtils.IsComponent(commandHandlerType)) {
                    throw new Exception(string.Format("{0} should be marked as component.", commandHandlerType.FullName));
                }
                var handlerTypes = commandHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));

                foreach (var handlerType in handlerTypes) {
                    var commandType = handlerType.GetGenericArguments().Single();
                    var commandHandlerWrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(commandType);
                    var commandHandler = ObjectContainer.Resolve(commandHandlerType);
                    var commandHandlerWrapper = Activator.CreateInstance(commandHandlerWrapperType, new object[] { commandHandler }) as ICommandHandler;
                    RegisterCommandHandler(commandType, commandHandlerWrapper);
                }
            }
        }
        private void RegisterCommandHandler(Type commandType, ICommandHandler commandHandler) {
            _commandHandlerDict[commandType] = commandHandler;
        }
    }
}
