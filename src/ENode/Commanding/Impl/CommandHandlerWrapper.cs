using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class CommandHandlerWrapper<TCommand> : ICommandHandler where TCommand : class, ICommand
    {
        private readonly ICommandHandler<TCommand> _commandHandler;

        public CommandHandlerWrapper(ICommandHandler<TCommand> commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public void Handle(ICommandContext context, object command)
        {
            _commandHandler.Handle(context, command as TCommand);
        }
        public object GetInnerHandler()
        {
            return _commandHandler;
        }
    }
}
