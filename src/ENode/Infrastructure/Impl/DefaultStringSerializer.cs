using System;

namespace ENode.Infrastructure {
    public class DefaultStringSerializer : IStringSerializer {
        private IBinarySerializer _binarySerializer;

        public DefaultStringSerializer(IBinarySerializer binarySerializer) {
            _binarySerializer = binarySerializer;
        }

        public string Serialize(object obj) {
            return Convert.ToBase64String(_binarySerializer.Serialize(obj));
        }
        public object Deserialize(string data) {
            return _binarySerializer.Deserialize(Convert.FromBase64String(data));
        }
        public T Deserialize<T>(string data) where T : class {
            return _binarySerializer.Deserialize<T>(Convert.FromBase64String(data));
        }
    }
}
