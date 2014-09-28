using System.Collections.Generic;
using ENode.Commanding;

namespace ENode.Eventing
{
    public interface IEventService
    {
        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Commit the given aggregate's domain events to the eventstore and publish the domain events.
        /// </summary>
        /// <param name="context"></param>
        void CommitEvent(EventCommittingContext context);
        /// <summary>Publish the given aggregate's domain events.
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
