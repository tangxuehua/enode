namespace ENode.Messaging.Impl
{
    /// <summary>The abstract base implementation of IMessageExecutor.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageExecutor<TMessage> : IMessageExecutor<TMessage> where TMessage : class, IMessage
    {
        /// <summary>Execute the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        public abstract void Execute(TMessage message, IMessageQueue<TMessage> queue);
    }
}
