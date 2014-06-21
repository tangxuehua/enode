using System.Collections.Generic;
using ENode.Commanding;
using ENode.Domain;

namespace ENode.Eventing
{
    /// <summary>Represents a context for event handler handling domain event.
    /// </summary>
    public interface IEventContext
    {
        /// <summary>Represents the current business process id.
        /// </summary>
        string ProcessId { get; }
        /// <summary>Represents the extension information of the current event.
        /// This information is from the corresponding command of the current event.
        /// </summary>
        IDictionary<string, string> Items { get; }
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        T Get<T>(object aggregateRootId) where T : class, IAggregateRoot;
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
