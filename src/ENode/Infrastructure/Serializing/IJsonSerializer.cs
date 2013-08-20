namespace ENode.Infrastructure.Serializing
{
    /// <summary>Represents a serializer to support json serialization or deserialization.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>Serialize an object to json string.
        /// </summary>
        string Serialize(object obj);
        /// <summary>Deserialize a json string to object.
        /// </summary>
        object Deserialize(string value);
        /// <summary>Deserialize a json string to a strong type object.
        /// </summary>
        T Deserialize<T>(string value) where T : class;
    }
}
