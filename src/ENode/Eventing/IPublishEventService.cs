using ENode.Commanding;

namespace ENode.Eventing
{
    public interface IPublishEventService
    {
        void PublishEvent(EventStream eventStream, ProcessingCommand processingCommand);
    }
}
