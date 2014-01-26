namespace ENode.Eventing
{
    public interface ICommitEventService
    {
        void CommitEvent(EventCommittingContext context);
    }
}
