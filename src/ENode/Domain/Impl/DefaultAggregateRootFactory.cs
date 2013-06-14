using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace ENode.Domain
{
    public class DefaultAggregateRootFactory : IAggregateRootFactory
    {
        private ConcurrentDictionary<Type, ConstructorInfo> _constructorInfoDict = new ConcurrentDictionary<Type, ConstructorInfo>();
        private BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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

                constructor = aggregateRootType.GetConstructor(_flags, null, Type.EmptyTypes, null);
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
