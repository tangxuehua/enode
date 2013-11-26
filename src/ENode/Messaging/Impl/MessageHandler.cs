namespace ENode.Messaging.Impl
{
    /// <summary>The abstract implementation of IMessageHandler.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageHandler<TMessagePayload> : IMessageHandler<TMessagePayload> where TMessagePayload : class, IPayload
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message"></param>
        public abstract void Handle(Message<TMessagePayload> message);
    }
}
