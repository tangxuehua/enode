using System;

namespace ENode.Domain
{
    public class AggregateRootReferenceChangedException : Exception
    {
        private const string ExceptionMessage = "Aggregate root [type={0},id={1}] reference already changed.";

        public IAggregateRoot AggregateRoot { get; set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public AggregateRootReferenceChangedException(IAggregateRoot aggregateRoot) : base(string.Format(ExceptionMessage, aggregateRoot.GetType().Name, aggregateRoot.UniqueId))
        {
            AggregateRoot = aggregateRoot;
        }
    }
}
