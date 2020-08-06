using System;

namespace ENode.Eventing
{
    public class DuplicateEventStreamException : Exception
    {
        private const string ExceptionMessage = "Aggregate root [type={0},id={1}] event stream already exist in the EventCommittingContextMailBox, eventStreamId: {2}";

        public DomainEventStream DomainEventStream { get; set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public DuplicateEventStreamException(DomainEventStream domainEventStream) : base(string.Format(ExceptionMessage, domainEventStream.AggregateRootTypeName, domainEventStream.AggregateRootId, domainEventStream.Version))
        {
            DomainEventStream = domainEventStream;
        }
    }
}
