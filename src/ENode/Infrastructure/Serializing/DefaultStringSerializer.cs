using System;

namespace ENode.Infrastructure.Serializing
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultStringSerializer : IStringSerializer
    {
        private readonly IBinarySerializer _binarySerializer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binarySerializer"></param>
        public DefaultStringSerializer(IBinarySerializer binarySerializer)
        {
            _binarySerializer = binarySerializer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string Serialize(object obj)
        {
            return Convert.ToBase64String(_binarySerializer.Serialize(obj));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object Deserialize(string data)
        {
            return _binarySerializer.Deserialize(Convert.FromBase64String(data));
        }
        /// <summary>
        /// 
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
