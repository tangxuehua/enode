using System.Collections.Generic;
using ENode.Commanding;

namespace ENode.Eventing
{
    public interface IEventService
    {
        /// <summary>Start the event service.
        /// </summary>
        void Start();
        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Add an event committing context to queue, and it will be process asynchronously.
        /// </summary>
        /// <param name="context"></param>
        void AddEventCommittingContextToQueue(EventCommittingContext context);
        /// <summary>Publish the given domain events.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <param name="eventStream"></param>
        void PublishDomainEvent(ProcessingCommand processingCommand, DomainEventStream eventStream);
        /// <summary>Publish the given events.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <param name="eventStream"></param>
        void PublishEvent(ProcessingCommand processingCommand, EventStream eventStream);
    }
}
