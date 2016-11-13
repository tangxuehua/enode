namespace ENode.Infrastructure
{
    public interface IProcessingMessage<X, Y> where X : class, IProcessingMessage<X, Y> where Y : IMessage
    {
        Y Message { get; }
        void SetMailbox(ProcessingMessageMailbox<X, Y> mailbox);
        void Complete();
    }
}
