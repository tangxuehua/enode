using System;
using System.Runtime.Serialization;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateRootFactory : IAggregateRootFactory
    {
        public IAggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            return FormatterServices.GetUninitializedObject(aggregateRootType) as IAggregateRoot;
        }
    }
}
