using System.Collections.Generic;
using ENode.Commanding;

namespace ENode.Eventing
{
    /// <summary>Represents a context for event handler handling domain event.
    /// </summary>
    public interface IEventContext
    {
        /// <summary>Add a to be execute command in the context.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        void AddCommand(ICommand command);
        /// <summary>Get all the to be execute commands from the context.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICommand> GetCommands();
    }
}
