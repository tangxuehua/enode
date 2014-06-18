using ENode.Commanding;

namespace ENode.Eventing
{
    public interface ICommitEventService
    {
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        void CommitEvent(EventProcessingContext context);
    }
}
