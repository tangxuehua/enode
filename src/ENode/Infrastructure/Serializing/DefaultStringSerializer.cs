using System;

namespace ENode.Infrastructure.Serializing
{
    /// <summary>The default implementation of IStringSerializer.
    /// </summary>
    public class DefaultStringSerializer : IStringSerializer
    {
        private readonly IBinarySerializer _binarySerializer;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="binarySerializer"></param>
        public DefaultStringSerializer(IBinarySerializer binarySerializer)
        {
            _binarySerializer = binarySerializer;
        }
        /// <summary>Serialize an object to string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string Serialize(object obj)
        {
            return Convert.ToBase64String(_binarySerializer.Serialize(obj));
        }
        /// <summary>Deserialize an object from a string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object Deserialize(string data)
        {
            return _binarySerializer.Deserialize(Convert.FromBase64String(data));
        }
        /// <summary>Deserialize a typed object from a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T Deserialize<T>(string data) where T : class
        {
            return _binarySerializer.Deserialize<T>(Convert.FromBase64String(data));
        }
    }
}
