using ENode.Commanding;
using ENode.Domain;

namespace ENode.Eventing
{
    public interface ICommitEventService
    {
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        void CommitEvent(EventProcessingContext context);
    }
}
