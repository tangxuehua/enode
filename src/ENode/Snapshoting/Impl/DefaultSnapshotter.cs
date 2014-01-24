using System;
using ECommon.Serializing;
using ENode.Domain;

namespace ENode.Snapshoting.Impl
{
    /// <summary>The default implementation of ISnapshotter.
    /// </summary>
    public class DefaultSnapshotter : ISnapshotter
    {
        #region Private Variables

        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IBinarySerializer _binarySerializer;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="binarySerializer"></param>
        public DefaultSnapshotter(IAggregateRootFactory aggregateRootFactory, IAggregateRootTypeProvider aggregateRootTypeProvider, IBinarySerializer binarySerializer)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _binarySerializer = binarySerializer;
        }

        #endregion

        /// <summary>Create snapshot for the given aggregate root.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        /// <returns></returns>
        public Snapshot CreateSnapshot(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }

            var payload = _binarySerializer.Serialize(aggregateRoot);
            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRoot.GetType());

            return new Snapshot(aggregateRootName, aggregateRoot.UniqueId, aggregateRoot.Version, payload, DateTime.UtcNow);
        }
        /// <summary>Restore the aggregate root from the given snapshot.
        /// </summary>
        /// <param name="snapshot"></param>
        /// <returns></returns>
        public IAggregateRoot RestoreFromSnapshot(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException("snapshot"); ;
            }

            var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(snapshot.AggregateRootName);
            return _binarySerializer.Deserialize(snapshot.Payload, aggregateRootType) as IAggregateRoot;
        }
    }
}
