using ENode.Domain;
using ENode.Eventing;

namespace ENode.Commanding
{
    /// <summary>Represents a command context for command handler handling command.
    /// </summary>
    public interface ICommandContext
    {
        /// <summary>Add a new aggregate into the current command context.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        void Add(IAggregateRoot aggregateRoot);
        /// <summary>Get an aggregate from memory cache, if not exist, then get it from event store.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(object id) where T : class, IAggregateRoot;
        /// <summary>Add a new event into the current command context.
        /// </summary>
        /// <param name="evnt"></param>
        void Add(IEvent evnt);
    }
}
