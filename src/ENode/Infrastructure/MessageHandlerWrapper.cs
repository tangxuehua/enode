namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IMessageHandler.
    /// </summary>
    public class MessageHandlerWrapper<TMessageContext, TMessage, TMessageInterface> : IMessageHandler
        where TMessageContext : class
        where TMessageInterface : class
        where TMessage : class
    {
        private readonly IMessageHandler<TMessageContext, TMessage> _messageHandler;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="messageHandler"></param>
        public MessageHandlerWrapper(IMessageHandler<TMessageContext, TMessage> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void Handle(object context, object message)
        {
            _messageHandler.Handle(context as TMessageContext, message as TMessage);
        }
        public object GetInnerHandler()
        {
            return _messageHandler;
        }
    }
}
