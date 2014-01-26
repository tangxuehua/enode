using ENode.Commanding;

namespace ENode.Eventing
{
    public interface IPublishEventService
    {
        void PublishEvent(EventStream eventStream, ICommand command, ICommandExecuteContext commandExecuteContext);
    }
}
