namespace ENode.Commanding
{
    /// <summary>A scheduler to schedule command to execute.
    /// </summary>
    public interface ICommandScheduler
    {
        /// <summary>Schedules a command to execute.
        /// </summary>
        /// <param name="command"></param>
        void ScheduleCommand(ProcessingCommand command);
        /// <summary>Schedules a command mailbox to execute.
        /// </summary>
        /// <param name="mailbox"></param>
        void ScheduleCommandMailbox(CommandMailbox mailbox);
    }
}
