using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class CommandHandlerProxy<TCommand> : ICommandHandlerProxy where TCommand : class, ICommand
    {
        private readonly ICommandHandler<TCommand> _commandHandler;

        public CommandHandlerProxy(ICommandHandler<TCommand> commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public void Handle(ICommandContext context, ICommand command)
        {
            _commandHandler.Handle(context, command as TCommand);
        }
        public object GetInnerHandler()
        {
            return _commandHandler;
        }
    }
}
