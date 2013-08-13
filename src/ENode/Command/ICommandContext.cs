using ENode.Domain;

namespace ENode.Commanding
{
    /// <summary>Represents a context environment for command handler handling command.
    /// </summary>
    public interface ICommandContext
    {
        /// <summary>Add a new aggregate into the current context.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        void Add(AggregateRoot aggregateRoot);
        /// <summary>Get an aggregate from the current context.
        /// <remarks>
        /// 1. If the aggregate already exist in the current context, then return it directly;
        /// 2. If not exist then try to get it from memory cache;
        /// 3. If still not exist then try to get it from event store;
        /// Finally, if the specified aggregate not found, then AggregateRootNotFoundException will be raised; otherwise, return the found aggregate.
        /// </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(object id) where T : AggregateRoot;
        /// <summary>Get an aggregate from the current context.
        /// <remarks>
        /// 1. If the aggregate already exist in the current context, then return it directly;
        /// 2. If not exist then try to get it from memory cache;
        /// 3. If still not exist then try to get it from event store;
        /// Finally, if the specified aggregate not found, return null; otherwise, return the found aggregate.
        /// </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetOrDefault<T>(object id) where T : AggregateRoot;
    }
}
