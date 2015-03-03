using System;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Snapshoting.Impl
{
    public class DefaultSnapshotter : ISnapshotter
    {
        #region Private Variables

        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly ITypeCodeProvider _aggregateRootTypeCodeProvider;
        private readonly IBinarySerializer _binarySerializer;

        #endregion

        #region Constructors

        public DefaultSnapshotter(IAggregateRootFactory aggregateRootFactory, ITypeCodeProvider aggregateRootTypeCodeProvider, IBinarySerializer binarySerializer)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
            _binarySerializer = binarySerializer;
        }

        #endregion

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
