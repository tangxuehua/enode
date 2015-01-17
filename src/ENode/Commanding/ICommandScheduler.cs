namespace ENode.Commanding
{
    public interface ICommandScheduler
    {
        void ScheduleCommand(ProcessingCommand command);
        void ScheduleCommandMailbox(CommandMailbox mailbox);
    }
}
