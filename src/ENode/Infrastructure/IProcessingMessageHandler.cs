namespace ENode.Infrastructure
{
    public interface IProcessingMessageHandler<X, Y>
        where X : class, IProcessingMessage<X, Y>
        where Y : IMessage
    {
        void HandleAsync(X processingMessage);
    }
}
