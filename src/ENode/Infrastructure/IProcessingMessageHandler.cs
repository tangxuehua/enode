namespace ENode.Infrastructure
{
    public interface IProcessingMessageHandler<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        void HandleAsync(X processingMessage);
    }
}
