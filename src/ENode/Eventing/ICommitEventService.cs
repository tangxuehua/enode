using ENode.Commanding;
namespace ENode.Eventing
{
    public interface ICommitEventService
    {
        void CommitEvent(EventStream eventStream, ProcessingCommand processingCommand);
    }
}
