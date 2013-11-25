namespace EQueue
{
    public interface IMessageListener
    {
        void ConsumeMessage(Message message, ConsumeContext context);
    }
}
