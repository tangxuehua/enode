namespace ENode.Commanding
{
    public interface IProcessingCommandHandler
    {
        void HandleAsync(ProcessingCommand processingCommand);
    }
}
