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
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IBinarySerializer _binarySerializer;

        #endregion

        #region Constructors

        public DefaultSnapshotter(IAggregateRootFactory aggregateRootFactory, ITypeNameProvider typeNameProvider, IBinarySerializer binarySerializer)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _typeNameProvider = typeNameProvider;
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
            var typeName = _typeNameProvider.GetTypeName(aggregateRoot.GetType());

            return new Snapshot(typeName, aggregateRoot.UniqueId, aggregateRoot.Version, payload, DateTime.Now);
        }
        public IAggregateRoot RestoreFromSnapshot(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException("snapshot"); ;
            }

            var aggregateRootType = _typeNameProvider.GetType(snapshot.AggregateRootTypeName);
            return _binarySerializer.Deserialize(snapshot.Payload, aggregateRootType) as IAggregateRoot;
        }
    }
}
