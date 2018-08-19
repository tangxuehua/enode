using System;
using System.Threading.Tasks;
using ECommon.Components;

namespace ENode.Commanding.Impl
{
    public class CommandHandlerProxy<TCommand> : ICommandHandlerProxy where TCommand : class, ICommand
    {
        private readonly ICommandHandler<TCommand> _commandHandler;
        private readonly Type _commandHandlerType;

        public CommandHandlerProxy(ICommandHandler<TCommand> commandHandler, Type commandHandlerType)
        {
            _commandHandler = commandHandler;
            _commandHandlerType = commandHandlerType;
        }

        public Task HandleAsync(ICommandContext context, ICommand command)
        {
            var handler = GetInnerObject() as ICommandHandler<TCommand>;
            return handler.HandleAsync(context, command as TCommand);
        }
        public object GetInnerObject()
        {
            if (_commandHandler != null)
            {
                return _commandHandler;
            }
            return ObjectContainer.Resolve(_commandHandlerType);
        }
    }
}
