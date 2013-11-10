using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of command context interface and tracking context interface.
    /// </summary>
    public class DefaultCommandContext : ICommandContext, ITrackingContext
    {
        private readonly IList<IAggregateRoot> _trackingAggregateRoots;
        private readonly IRepository _repository;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="repository"></param>
        public DefaultCommandContext(IRepository repository)
        {
            _trackingAggregateRoots = new List<IAggregateRoot>();
            _repository = repository;
        }

        /// <summary>Add an aggregate root to the context.
        /// </summary>
        /// <param name="aggregateRoot">The aggregate root to add.</param>
        /// <exception cref="ArgumentNullException">Throwed when the aggregate root is null.</exception>
        public void Add(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }

            _trackingAggregateRoots.Add(aggregateRoot);
        }
        /// <summary>Get the aggregate from the context.
        /// </summary>
        /// <param name="id">The id of the aggregate root.</param>
        /// <typeparam name="T">The type of the aggregate root.</typeparam>
        /// <returns>The found aggregate root.</returns>
        /// <exception cref="ArgumentNullException">Throwed when the id is null.</exception>
        /// <exception cref="AggregateRootNotFoundException">Throwed when the aggregate root not found.</exception>
        public T Get<T>(object id) where T : class, IAggregateRoot
        {
            var aggregateRoot = GetOrDefault<T>(id);

            if (aggregateRoot == null)
            {
                throw new AggregateRootNotFoundException(id, typeof(T));
            }

            return aggregateRoot;
        }
        /// <summary>Get the aggregate from the context, if the aggregate root not exist, returns null.
        /// </summary>
        /// <param name="id">The id of the aggregate root.</param>
        /// <typeparam name="T">The type of the aggregate root.</typeparam>
        /// <returns>If the aggregate root was found, then returns it; otherwise, returns null.</returns>
        /// <exception cref="ArgumentNullException">Throwed when the id is null.</exception>
        public T GetOrDefault<T>(object id) where T : class, IAggregateRoot
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            var aggregateRoot = _repository.Get<T>(id);

            if (aggregateRoot != null)
            {
                _trackingAggregateRoots.Add(aggregateRoot);
            }

            return aggregateRoot;
        }
        /// <summary>Returns all the tracked aggregate roots of the current context.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
        {
            return _trackingAggregateRoots;
        }
        /// <summary>Clear all the tracked aggregate roots of the current context.
        /// </summary>
        public void Clear()
        {
            _trackingAggregateRoots.Clear();
        }
    }
}
