namespace ENode.Infrastructure.Serializing
{
    /// <summary>Represents a serializer to serialize object to byte array.
    /// </summary>
    public interface IBinarySerializer
    {
        /// <summary>Serialize an object to byte array.
        /// </summary>
        byte[] Serialize(object obj);
        /// <summary>Deserialize an object from a byte array.
        /// </summary>
        object Deserialize(byte[] data);
        /// <summary>Deserialize a typed object from a byte array.
        /// </summary>
        T Deserialize<T>(byte[] data) where T : class;
    }
}
