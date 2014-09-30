namespace ENode.Eventing
{
    /// <summary>Represents a message handler.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        void Handle(object context, object message);
        /// <summary>Get the inner message handler.
        /// </summary>
        /// <returns></returns>
        object GetInnerHandler();
    }
    /// <summary>Represents a message handler.
    /// </summary>
    /// <typeparam name="TMessageHandleContext"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<TMessageHandleContext, in TMessage>
        where TMessageHandleContext : class
        where TMessage : class
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        void Handle(TMessageHandleContext context, TMessage message);
    }
}
