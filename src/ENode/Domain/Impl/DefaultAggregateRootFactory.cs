using System;
using System.Runtime.Serialization;

namespace ENode.Domain.Impl
{
    /// <summary>The default implementation of IAggregateRootFactory.
    /// </summary>
    public class DefaultAggregateRootFactory : IAggregateRootFactory
    {
        /// <summary>Create an empty aggregate root with the given type.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            return FormatterServices.GetUninitializedObject(aggregateRootType) as AggregateRoot;
        }
    }
}
