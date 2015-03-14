namespace ENode.Infrastructure
{
    public interface IProcessingMessageScheduler<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        void SetConcurrencyLevel(int concurrentLevel);
        void ScheduleMessage(X processingMessage);
        void ScheduleMailbox(ProcessingMessageMailbox<X, Y, Z> mailbox);
    }
}
