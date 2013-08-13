using System;

namespace ENode.Commanding
{
    /// <summary>Represents a provider which provide the command handler for command.
    /// </summary>
    public interface ICommandHandlerProvider
    {
        /// <summary>Get the command handler for the given command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        ICommandHandler GetCommandHandler(ICommand command);
        /// <summary>Check whether a given type is a command handler type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsCommandHandler(Type type);
    }
}
