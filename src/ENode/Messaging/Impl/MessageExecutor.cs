namespace ENode.Messaging.Impl
{
    /// <summary>The abstract implementation of IMessageExecutor.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageExecutor<TMessagePayload> : IMessageExecutor<TMessagePayload> where TMessagePayload : class, IMessagePayload
    {
        /// <summary>Execute the given message.
        /// </summary>
        /// <param name="message"></param>
        public abstract void Execute(Message<TMessagePayload> message);
    }
}
