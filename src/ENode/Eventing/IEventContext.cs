using System.Collections.Generic;
using ENode.Commanding;
using ENode.Domain;

namespace ENode.Eventing
{
    /// <summary>Represents a context for event handler handling event.
    /// </summary>
    public interface IEventContext
    {
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
        /// <summary>Add a to be execute process command in the context.
        /// </summary>
        /// <param name="command">The process command to execute.</param>
        void AddCommand(ICommand command);
        /// <summary>Get all the to be execute process commands from the context.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICommand> GetCommands();
    }
}
