using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ENode.Infrastructure.Serializing
{
    /// <summary>Defines a serializer to serialize object to byte array.
    /// </summary>
    public class DefaultBinarySerializer : IBinarySerializer
    {
        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        /// <summary>Serialize an object to byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                _binaryFormatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }
        /// <summary>Deserialize an object from a byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return _binaryFormatter.Deserialize(stream);
            }
        }
        /// <summary>Deserialize a typed object from a byte array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                return _binaryFormatter.Deserialize(stream) as T;
            }
        }
    }
}
