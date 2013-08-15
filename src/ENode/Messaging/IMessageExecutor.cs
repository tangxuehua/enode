namespace ENode.Messaging
{
    /// <summary>Represents a message executor.
    /// </summary>
    public interface IMessageExecutor<TMessage> where TMessage : class, IMessage
    {
        /// <summary>Execute the given queue message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        void Execute(TMessage message, IMessageQueue<TMessage> queue);
    }
}
