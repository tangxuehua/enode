using System.Collections.Generic;
using System.Reflection;
using ENode.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ENode.JsonNet {
    public class NewtonsoftJsonSerializer : IJsonSerializer {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter> { new IsoDateTimeConverter() },
            ContractResolver = new SisoJsonDefaultContractResolver()
        };

        public string Serialize(object obj) {
            if (obj == null) return null;
            return JsonConvert.SerializeObject(obj, _settings);
        }
        public object Deserialize(string value) {
            return JsonConvert.DeserializeObject(value, _settings);
        }
        public T Deserialize<T>(string value) where T : class {
            return JsonConvert.DeserializeObject<T>(JObject.Parse(value).ToString(), _settings);
        }
        public T Deserialize<T>(string value, T anonymousTypeObject) {
            return JsonConvert.DeserializeAnonymousType<T>(JObject.Parse(value).ToString(), anonymousTypeObject);
        }
    }

    public class SisoJsonDefaultContractResolver : DefaultContractResolver {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            if (!jsonProperty.Writable) {
                var property = member as PropertyInfo;
                if (property != null) {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    jsonProperty.Writable = hasPrivateSetter;
                }
            }

            return jsonProperty;
        }
    }
}
