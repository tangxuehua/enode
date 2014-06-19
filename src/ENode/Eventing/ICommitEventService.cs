using ENode.Commanding;

namespace ENode.Eventing
{
    public interface ICommitEventService
    {
        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        void SetCommandExecutor(ICommandExecutor commandExecutor);
        /// <summary>Commit the given aggregate's domain events to the eventstore and publish the domain events.
        /// </summary>
        /// <param name="context"></param>
        void CommitEvent(EventProcessingContext context);
    }
}
