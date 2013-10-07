using System;
using System.Text;
using ENode.Infrastructure.Serializing;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace ENode.Redis
{
    /// <summary>Redis based binary serializer implementation.
    /// </summary>
    public class RedisBinarySerializer : IBinarySerializer
    {
        /// <summary>Serialize an object to byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] Serialize(object obj)
        {
            return RedisClient.SerializeToUtf8Bytes(obj);
        }
        /// <summary>Deserialize an object from a byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Deserialize(byte[] data, Type type)
        {
            return JsonSerializer.DeserializeFromString(Encoding.UTF8.GetString(data), type);
        }
        /// <summary>Deserialize a typed object from a byte array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] data) where T : class
        {
            return JsonSerializer.DeserializeFromString<T>(Encoding.UTF8.GetString(data));
        }
    }
}
