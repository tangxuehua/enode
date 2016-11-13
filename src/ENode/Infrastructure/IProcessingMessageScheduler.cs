namespace ENode.Infrastructure
{
    public interface IProcessingMessageScheduler<X, Y>
        where X : class, IProcessingMessage<X, Y>
        where Y : IMessage
    {
        void ScheduleMessage(X processingMessage);
        void ScheduleMailbox(ProcessingMessageMailbox<X, Y> mailbox);
    }
}
