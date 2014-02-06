using ENode.Commanding;

namespace ENode.Eventing
{
    public interface IPublishEventService
    {
        void PublishEvent(EventProcessingContext context);
    }
}
