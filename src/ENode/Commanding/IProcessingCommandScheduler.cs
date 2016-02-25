namespace ENode.Commanding
{
    public interface IProcessingCommandScheduler
    {
        void ScheduleMailbox(ProcessingCommandMailbox mailbox);
    }
}
