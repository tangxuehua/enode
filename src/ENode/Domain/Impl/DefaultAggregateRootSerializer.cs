using System;
using ECommon.Serializing;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateRootSerializer : IAggregateRootSerializer
    {
        private readonly IBinarySerializer _binarySerializer;

        public DefaultAggregateRootSerializer(IBinarySerializer binarySerializer)
        {
            _binarySerializer = binarySerializer;
        }

        public byte[] Serialize(IAggregateRoot aggregateRoot)
        {
            return _binarySerializer.Serialize(aggregateRoot);
        }
        public IAggregateRoot Deserialize(byte[] data, Type aggregateRootType)
        {
            return _binarySerializer.Deserialize(data, aggregateRootType) as IAggregateRoot;
        }
        public T Deserialize<T>(byte[] data) where T : class, IAggregateRoot
        {
            return _binarySerializer.Deserialize<T>(data);
        }
    }
}
