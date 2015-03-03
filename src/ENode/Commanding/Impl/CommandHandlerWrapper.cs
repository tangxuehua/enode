using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class CommandProxyHandler<TCommand> : ICommandHandler where TCommand : class, ICommand
    {
        private readonly ICommandHandler<TCommand> _commandHandler;

        public CommandProxyHandler(ICommandHandler<TCommand> commandHandler)
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
