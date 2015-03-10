namespace ENode.Infrastructure
{
    public interface IProcessingMessageHandler<X, Y, Z> where X : class, IProcessingMessage<X, Y, Z>
    {
        void HandleAsync(X processingMessage);
    }
}
