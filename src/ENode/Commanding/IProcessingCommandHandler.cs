namespace ENode.Commanding
{
    public interface IProcessingCommandHandler
    {
        void Handle(ProcessingCommand processingCommand);
    }
}
