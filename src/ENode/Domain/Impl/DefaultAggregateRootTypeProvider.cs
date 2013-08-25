using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    /// <summary>Default implementation of IAggregateRootTypeProvider and IAssemblyInitializer.
    /// </summary>
    public class DefaultAggregateRootTypeProvider : IAggregateRootTypeProvider, IAssemblyInitializer
    {
        private readonly IDictionary<string, Type> _mappings = new Dictionary<string, Type>();

        /// <summary>Initialize from the given assemblies.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <exception cref="Exception"></exception>
        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(TypeUtils.IsAggregateRoot))
                {
                    if (!type.IsSerializable)
                    {
                        throw new Exception(string.Format("{0} should be marked as serializable.", type.FullName));
                    }
                    _mappings.Add(type.FullName, type);
                }
            }
        }
        /// <summary>Get the aggregate root type name by the aggregate root type.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetAggregateRootTypeName(Type aggregateRootType)
        {
            return _mappings.Single(x => x.Value == aggregateRootType).Key;
        }
        /// <summary>Get the aggregate root type by the aggregate root type name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Type GetAggregateRootType(string name)
        {
            return _mappings.ContainsKey(name) ? _mappings[name] : null;
        }
        /// <summary>Get all the aggregate root types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Type> GetAllAggregateRootTypes()
        {
            return _mappings.Values;
        }
    }
}
