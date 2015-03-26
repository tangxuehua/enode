namespace ENode.Infrastructure
{
    public interface IProcessingMessage<X, Y, Z> where X : class, IProcessingMessage<X, Y, Z> where Y : IMessage
    {
        Y Message { get; }
        void SetMailbox(ProcessingMessageMailbox<X, Y, Z> mailbox);
        void HandleLater();
        void Complete(Z result);
    }
}
