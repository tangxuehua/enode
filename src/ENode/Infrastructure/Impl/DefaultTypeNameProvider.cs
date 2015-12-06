using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace ENode.Infrastructure.Impl
{
    public class DefaultTypeNameProvider : ITypeNameProvider, IAssemblyInitializer
    {
        private Assembly[] _asemblies;
        private readonly ConcurrentDictionary<string, Type> _dict = new ConcurrentDictionary<string, Type>();

        public Type GetType(string typeName)
        {
            Type t;
            if (_dict.TryGetValue(typeName, out t))
            {
                return t;
            }
            foreach (var assembly in _asemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    _dict.TryAdd(typeName, type);
                    return type;
                }
            }
            return null;
        }
        public string GetTypeName(Type type)
        {
            return type.FullName;
        }
        public void Initialize(params Assembly[] assemblies)
        {
            _asemblies = assemblies;
        }
    }
}
