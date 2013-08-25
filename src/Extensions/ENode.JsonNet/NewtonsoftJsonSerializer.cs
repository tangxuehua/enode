using System.Collections.Generic;
using System.Reflection;
using ENode.Infrastructure.Serializing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ENode.JsonNet
{
    /// <summary>Json.Net implementationof IJsonSerializer.
    /// </summary>
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter> { new IsoDateTimeConverter() },
            ContractResolver = new SisoJsonDefaultContractResolver()
        };
        /// <summary>Serialize an object to json string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string Serialize(object obj)
        {
            return obj == null ? null : JsonConvert.SerializeObject(obj, Settings);
        }
        /// <summary>Deserialize a json string to object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Deserialize(string value)
        {
            return JsonConvert.DeserializeObject(value, Settings);
        }
        /// <summary>Deserialize a json string to a strong type object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Deserialize<T>(string value) where T : class
        {
            return JsonConvert.DeserializeObject<T>(JObject.Parse(value).ToString(), Settings);
        }
    }

    /// <summary>
    /// </summary>
    public class SisoJsonDefaultContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            if (jsonProperty.Writable) return jsonProperty;
            var property = member as PropertyInfo;
            if (property == null) return jsonProperty;
            var hasPrivateSetter = property.GetSetMethod(true) != null;
            jsonProperty.Writable = hasPrivateSetter;

            return jsonProperty;
        }
    }
}
