using System.Collections.Generic;
using ENode.Commanding;
using ENode.Domain;

namespace ENode.Infrastructure
{
    /// <summary>Represents a data handling context.
    /// </summary>
    public interface IHandlingContext
    {
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        T Get<T>(object aggregateRootId) where T : class, IAggregateRoot;
        /// <summary>Add a command into the context, and the command will be send later.
        /// </summary>
        /// <param name="command">The command.</param>
        void AddCommand(ICommand command);
        /// <summary>Get all the commands in the context.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICommand> GetCommands();
    }
}
