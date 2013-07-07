namespace ENode.Commanding
{
    public class CommandHandlerWrapper<T> : ICommandHandler where T : class, ICommand
    {
        private ICommandHandler<T> _commandHandler;

        public CommandHandlerWrapper(ICommandHandler<T> commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public void Handle(ICommandContext context, ICommand command)
        {
            _commandHandler.Handle(context, command as T);
        }
        public object GetInnerCommandHandler()
        {
            return _commandHandler;
        }
    }
}
