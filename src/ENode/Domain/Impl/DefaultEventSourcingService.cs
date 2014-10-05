using System.Collections.Generic;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultEventSourcingService : IEventSourcingService
    {
        private readonly IAggregateRootInternalHandlerProvider _eventHandlerProvider;

        public DefaultEventSourcingService(IAggregateRootInternalHandlerProvider eventHandlerProvider)
        {
            _eventHandlerProvider = eventHandlerProvider;
        }

        public void ReplayEvents(IAggregateRoot aggregateRoot, DomainEventStream eventStream)
        {
            VerifyEvent(aggregateRoot, eventStream);
            foreach (var evnt in eventStream.DomainEvents)
            {
                HandleEvent(aggregateRoot, evnt);
            }
            aggregateRoot.IncreaseVersion();
            if (aggregateRoot.Version != eventStream.Version)
            {
                throw new ENodeException("Aggregate root version mismatch, aggregateId:{0}, current version:{1}, expected version:{2}", aggregateRoot.UniqueId, aggregateRoot.Version, eventStream.Version);
            }
        }
        public void ReplayEvents(IAggregateRoot aggregateRoot, IEnumerable<DomainEventStream> eventStreams)
        {
            foreach (var eventStream in eventStreams)
            {
                ReplayEvents(aggregateRoot, eventStream);
            }
        }

        private void HandleEvent(IAggregateRoot aggregateRoot, IDomainEvent evnt)
        {
            var handler = _eventHandlerProvider.GetInternalEventHandler(aggregateRoot.GetType(), evnt.GetType());
            if (handler == null)
            {
                throw new ENodeException("Could not find event handler for [{0}] of [{1}]", evnt.GetType().FullName, aggregateRoot.GetType().FullName);
            }

            handler(aggregateRoot, evnt);
        }
        private void VerifyEvent(IAggregateRoot aggregateRoot, DomainEventStream eventStream)
        {
            if (eventStream.Version > 1 && eventStream.AggregateRootId != aggregateRoot.UniqueId)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate root as the AggregateRootId not matched. EventStream aggregateRootId:{0}, current aggregateRootId:{1}",
                                        eventStream.AggregateRootId,
                                        aggregateRoot.UniqueId);
                throw new ENodeException(errorMessage);
            }

            if (eventStream.Version != aggregateRoot.Version + 1)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate root as the version not matched. EventStream version:{0}, current aggregateRoot version:{1}",
                                        eventStream.Version,
                                        aggregateRoot.Version);
                throw new ENodeException(errorMessage);
            }
        }
    }
}
