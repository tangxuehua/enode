namespace ENode.Commanding
{
    /// <summary>Represents a command handler interface.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>Handles the given command with the provided context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        void Handle(ICommandContext context, ICommand command);
        /// <summary>Returns the inner really command handler.
        /// </summary>
        /// <returns></returns>
        object GetInnerCommandHandler();
    }
    /// <summary>Represents a command handler interface.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
    {
        /// <summary>Handles the given command with the provided context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        void Handle(ICommandContext context, TCommand command);
    }
}
