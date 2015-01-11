using ENode.Infrastructure;

namespace ENode.Eventing
{
    public interface IEventDispatcher
    {
        void Start();
        void EnqueueProcessingContext(IProcessingContext processingContext);
        bool DispatchEventsToHandlers(EventStream eventStream);
        bool DispatchEventToHandlers(IEvent evnt);
    }
}
