namespace ENode.Infrastructure.Serializing
{
    /// <summary>Represents a serializer to serialize object to string.
    /// </summary>
    public interface IStringSerializer
    {
        /// <summary>Serialize an object to string.
        /// </summary>
        string Serialize(object obj);
        /// <summary>Deserialize an object from a string.
        /// </summary>
        object Deserialize(string data);
        /// <summary>Deserialize a typed object from a string.
        /// </summary>
        T Deserialize<T>(string data) where T : class;
    }
}
