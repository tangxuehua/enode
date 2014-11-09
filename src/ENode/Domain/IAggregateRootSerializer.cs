using System;

namespace ENode.Domain
{
    /// <summary>Represents an serializer to serialize or deserialize aggregate root.
    /// </summary>
    public interface IAggregateRootSerializer
    {
        /// <summary>Serialize the given aggregate root to binary data.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        /// <returns></returns>
        byte[] Serialize(IAggregateRoot aggregateRoot);
        /// <summary>Deserialize the given data to aggregate root.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        IAggregateRoot Deserialize(byte[] data, Type aggregateRootType);
        /// <summary>Deserialize the given data to aggregate root.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        T Deserialize<T>(byte[] data) where T : class, IAggregateRoot;
    }
}
