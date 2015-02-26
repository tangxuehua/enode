using ENode.Commanding;

namespace ENode.Eventing
{
    /// <summary>Represents a service to commit or publish events.
    /// </summary>
    public interface IEventService
    {
        /// <summary>Set the command executor for command retring.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Start the event service.
        /// </summary>
        void Start();
        /// <summary>Commit the given aggregate's domain events to the eventstore async and publish the domain events.
        /// </summary>
        /// <param name="context"></param>
        void CommitEventAsync(EventCommittingContext context);
        /// <summary>Publish the given domain events async.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <param name="eventStream"></param>
        void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStream eventStream);
        /// <summary>Publish the given events async.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <param name="eventStream"></param>
        void PublishEventAsync(ProcessingCommand processingCommand, EventStream eventStream);
    }
}
