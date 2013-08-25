using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace ENode.Domain.Impl
{
    /// <summary>The default implementation of IAggregateRootFactory.
    /// </summary>
    public class DefaultAggregateRootFactory : IAggregateRootFactory
    {
        private readonly ConcurrentDictionary<Type, ConstructorInfo> _constructorInfoDict = new ConcurrentDictionary<Type, ConstructorInfo>();
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>Create an empty aggregate root with the given type.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            ConstructorInfo constructor;

            if (_constructorInfoDict.ContainsKey(aggregateRootType))
            {
                constructor = _constructorInfoDict[aggregateRootType];
            }
            else
            {
                if (!typeof(AggregateRoot).IsAssignableFrom(aggregateRootType))
                {
                    throw new Exception(string.Format("Invalid aggregate root type {0}", aggregateRootType.FullName));
                }

                constructor = aggregateRootType.GetConstructor(Flags, null, Type.EmptyTypes, null);
                if (constructor == null)
                {
                    throw new Exception(string.Format("Could not found a default constructor on aggregate root type {0}", aggregateRootType.FullName));
                }

                _constructorInfoDict[aggregateRootType] = constructor;
            }

            return constructor.Invoke(null) as AggregateRoot;
        }
    }
}
