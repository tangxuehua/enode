using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ENode.Commanding
{
    public class DefaultCommandHandlerProvider : ICommandHandlerProvider
    {
        private ConcurrentDictionary<Type, ICommandHandler> _commandHandlerDict = new ConcurrentDictionary<Type, ICommandHandler>();

        public DefaultCommandHandlerProvider(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAllCommandHandlersInAssembly(assembly);
            }
        }
        public ICommandHandler GetCommandHandler(ICommand command)
        {
            ICommandHandler commandHandler;
            if (_commandHandlerDict.TryGetValue(command.GetType(), out commandHandler))
            {
                return commandHandler;
            }
            return null;
        }

        private void RegisterAllCommandHandlersInAssembly(Assembly assembly)
        {
            var targetType = typeof(ICommandHandler<>);
            var types = assembly.GetExportedTypes();
            var commandHandlerTypes = types.Where(x => x.IsInterface == false && x.IsAbstract == false && x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == targetType));

            foreach (var commandHandlerType in commandHandlerTypes)
            {
                var handlerTypes = commandHandlerType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == targetType);

                foreach (var handlerType in handlerTypes)
                {
                    var commandType = handlerType.GetGenericArguments().Single();
                    var commandHandlerWrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(commandType);
                    var commandHandler = Activator.CreateInstance(commandHandlerType);
                    var commandHandlerWrapper = Activator.CreateInstance(commandHandlerWrapperType, new object[] { commandHandler }) as ICommandHandler;
                    RegisterCommandHandler(commandType, commandHandlerWrapper);
                }
            }
        }
        private void RegisterCommandHandler(Type commandType, ICommandHandler commandHandler)
        {
            _commandHandlerDict[commandType] = commandHandler;
        }
    }
}
