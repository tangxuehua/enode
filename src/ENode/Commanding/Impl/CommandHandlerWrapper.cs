namespace ENode.Commanding.Impl
{
    /// <summary>A wrapper of command handler.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    public class CommandHandlerWrapper<T> : ICommandHandler where T : class, ICommand
    {
        private readonly ICommandHandler<T> _commandHandler;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandHandler"></param>
        public CommandHandlerWrapper(ICommandHandler<T> commandHandler)
        {
            _commandHandler = commandHandler;
        }

        /// <summary>Handles the given command with the provided context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        public void Handle(ICommandContext context, ICommand command)
        {
            _commandHandler.Handle(context, command as T);
        }
        /// <summary>Returns the inner really command handler.
        /// </summary>
        /// <returns></returns>
        public object GetInnerCommandHandler()
        {
            return _commandHandler;
        }
    }
}
