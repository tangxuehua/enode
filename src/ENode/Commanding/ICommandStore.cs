using System.Collections.Generic;

namespace ENode.Commanding
{
    /// <summary>Represents a command store to store all the handled commands.
    /// </summary>
    public interface ICommandStore
    {
        /// <summary>Add the given handled command to the command store.
        /// </summary>
        CommandAddResult AddCommand(HandledCommand handledCommand);
        /// <summary>Find a handled command by the commandId.
        /// </summary>
        /// <returns></returns>
        HandledCommand Find(string commandId);
        /// <summary>Remove a handled command by the commandId.
        /// </summary>
        /// <returns></returns>
        void Remove(string commandId);
    }
}
