using ENode.Commanding;

namespace ENode.Eventing
{
    /// <summary>Represents a service to commit or publish domain events.
    /// </summary>
    public interface IEventCommittingService
    {
        /// <summary>Commit the given aggregate's domain events to the eventstore async and publish the domain events.
        /// </summary>
        /// <param name="eventCommittingContext"></param>
        void CommitDomainEventAsync(EventCommittingContext eventCommittingContext);
        /// <summary>Publish the given domain event stream async.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <param name="eventStream"></param>
        void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStream eventStream);
    }
}
