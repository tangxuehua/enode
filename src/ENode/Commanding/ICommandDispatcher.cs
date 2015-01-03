namespace ENode.Commanding
{
    public interface ICommandDispatcher
    {
        int ExecuteCommandCountOfOneTask { get; }
        int TaskMaxDeadlineMilliseconds { get; }
        void RegisterCommandForExecution(ProcessingCommand command);
        void RegisterMailboxForExecution(CommandMailbox mailbox);
        void RegisterMailboxForDelayExecution(CommandMailbox mailbox, int delayMilliseconds);
    }
}
