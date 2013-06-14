namespace ENode.Commanding
{
    /// <summary>Represents a provider which can provide command handler for command.
    /// </summary>
    public interface ICommandHandlerProvider
    {
        /// <summary>Get the command handler for the given command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        ICommandHandler GetCommandHandler(ICommand command);
    }
}
