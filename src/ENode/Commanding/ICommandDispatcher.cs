namespace ENode.Commanding
{
    public interface ICommandDispatcher
    {
        void RegisterCommandForExecution(ProcessingCommand command);
        void RegisterMailboxForExecution(CommandMailbox mailbox);
    }
}
