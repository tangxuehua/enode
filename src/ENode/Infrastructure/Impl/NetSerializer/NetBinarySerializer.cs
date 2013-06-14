using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ENode.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;
using NetSerializer;

namespace ENode.ThirdParty
{
    public class NetBinarySerializer : IBinarySerializer
    {
        public NetBinarySerializer(params Assembly[] assemblies)
        {
            InitializeSerializableTypes(assemblies);
        }

        public byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize(stream);
            }
        }

        public T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize(stream) as T;
            }
        }

        private void InitializeSerializableTypes(params Assembly[] assemblies)
        {
            var internalSerializableTypes = new List<Type>
            {
                typeof(CommandInfo),
                typeof(EventStream)
            };

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(x => TypeUtils.IsSerializable(x)))
                {
                    if (!internalSerializableTypes.Contains(type))
                    {
                        internalSerializableTypes.Add(type);
                    }
                }
            }

            Serializer.Initialize(internalSerializableTypes.ToArray());
        }
    }
}
