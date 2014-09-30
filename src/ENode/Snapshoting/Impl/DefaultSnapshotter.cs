using System;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Snapshoting.Impl
{
    /// <summary>The default implementation of ISnapshotter.
    /// </summary>
    public class DefaultSnapshotter : ISnapshotter
    {
        #region Private Variables

        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly ITypeCodeProvider<IAggregateRoot> _aggregateRootTypeCodeProvider;
        private readonly IBinarySerializer _binarySerializer;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="aggregateRootTypeCodeProvider"></param>
        /// <param name="binarySerializer"></param>
        public DefaultSnapshotter(IAggregateRootFactory aggregateRootFactory, ITypeCodeProvider<IAggregateRoot> aggregateRootTypeCodeProvider, IBinarySerializer binarySerializer)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
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
            var aggregateRootTypeCode = _aggregateRootTypeCodeProvider.GetTypeCode(aggregateRoot.GetType());

            return new Snapshot(aggregateRootTypeCode, aggregateRoot.UniqueId, aggregateRoot.Version, payload, DateTime.Now);
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

            var aggregateRootType = _aggregateRootTypeCodeProvider.GetType(snapshot.AggregateRootTypeCode);
            return _binarySerializer.Deserialize(snapshot.Payload, aggregateRootType) as IAggregateRoot;
        }
    }
}
