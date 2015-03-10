namespace ENode.Infrastructure
{
    public interface IProcessingMessage<X, Y, Z> where X : class, IProcessingMessage<X, Y, Z>
    {
        Y Message { get; }
        void SetMailbox(ProcessingMessageMailbox<X, Y, Z> mailbox);
        void Complete(Z result);
    }
}
