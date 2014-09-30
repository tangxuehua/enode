using System.Collections.Generic;
using ENode.Commanding;
using ENode.Domain;

namespace ENode.Exceptions
{
    /// <summary>Represents a context for exception handler handling exception.
    /// </summary>
    public interface IExceptionHandlingContext
    {
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
