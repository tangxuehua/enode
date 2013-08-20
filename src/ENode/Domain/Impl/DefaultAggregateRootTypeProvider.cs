using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateRootTypeProvider : IAggregateRootTypeProvider, IAssemblyInitializer
    {
        private IDictionary<string, Type> _mappings = new Dictionary<string, Type>();

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => TypeUtils.IsAggregateRoot(t)))
                {
                    if (!type.IsSerializable)
                    {
                        throw new Exception(string.Format("{0} should be marked as serializable.", type.FullName));
                    }
                    _mappings.Add(type.FullName, type);
                }
            }
        }
        public string GetAggregateRootTypeName(Type aggregateRootType)
        {
            return _mappings.Single(x => x.Value == aggregateRootType).Key;
        }
        public Type GetAggregateRootType(string name)
        {
            if (_mappings.ContainsKey(name))
            {
                return _mappings[name];
            }
            return null;
        }
        public IEnumerable<Type> GetAllAggregateRootTypes()
        {
            return _mappings.Values;
        }
    }
}
