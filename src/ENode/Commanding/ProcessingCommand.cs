namespace ENode.Commanding
{
    public class ProcessingCommand
    {
        public ICommand Command { get; private set; }
        public ICommandExecuteContext CommandExecuteContext { get; private set; }
        public int RetriedCount { get; private set; }

        public ProcessingCommand(ICommand command, ICommandExecuteContext commandExecuteContext)
        {
            Command = command;
            CommandExecuteContext = commandExecuteContext;
        }

        public void IncreaseRetriedCount()
        {
            RetriedCount++;
        }
    }
}
