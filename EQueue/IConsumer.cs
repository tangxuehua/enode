namespace EQueue
{
    public interface IConsumer
    {
        void Start();
        void Shutdown();
        void Register(IMessageListener messageListener);
        void SendMessageBack(Message message);
        void Subscribe(string topic);
        PullResult Pull(MessageQueue messageQueue, long offset, int maxNums);
        void UpdateConsumeOffset(MessageQueue mq, long offset);
        long FetchConsumeOffset(MessageQueue mq, bool fromStore);
    }
}
