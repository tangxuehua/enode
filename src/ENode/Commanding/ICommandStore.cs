using System.Collections.Generic;

namespace ENode.Commanding
{
    /// <summary>Represents a command store to store all the handled commands.
    /// </summary>
    public interface ICommandStore
    {
        /// <summary>Add the given handled aggregate command to the command store.
        /// </summary>
        CommandAddResult AddHandledAggregateCommand(HandledAggregateCommand handledCommand);
        /// <summary>Find a handled aggregate command by the commandId.
        /// </summary>
        /// <returns></returns>
        HandledAggregateCommand FindHandledAggregateCommand(string commandId);
        /// <summary>Add the given handled command to the command store.
        /// </summary>
        CommandAddResult AddHandledCommand(HandledCommand handledCommand);
        /// <summary>Find a handled command by the commandId.
        /// </summary>
        /// <returns></returns>
        HandledCommand FindHandledCommand(string commandId);
        /// <summary>Remove a handled command by the commandId.
        /// </summary>
        /// <returns></returns>
        void Remove(string commandId);
    }
}
