using ENode.Commanding;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a service to commit or publish domain event stream.
    /// </summary>
    public interface IEventService
    {
        /// <summary>Set the command executor for command retring.
        /// </summary>
        /// <param name="processingCommandHandler"></param>
        void SetProcessingCommandHandler(IProcessingMessageHandler<ProcessingCommand, ICommand, CommandResult> processingCommandHandler);
        /// <summary>Start the event service.
        /// </summary>
        void Start();
        /// <summary>Commit the given aggregate's domain events to the eventstore async and publish the domain events.
        /// </summary>
        /// <param name="context"></param>
        void CommitDomainEventAsync(EventCommittingContext context);
        /// <summary>Publish the given domain event stream async.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <param name="eventStream"></param>
        void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStream eventStream);
    }
}
