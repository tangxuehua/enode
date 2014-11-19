using ENode.Infrastructure;

namespace ENode.Messaging.Impl
{
    public class MessageHandlerWrapper<TMessage> : IMessageHandler where TMessage : class, IMessage
    {
        private readonly IMessageHandler<TMessage> _messageHandler;

        public MessageHandlerWrapper(IMessageHandler<TMessage> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void Handle(IHandlingContext context, object message)
        {
            _messageHandler.Handle(context, message as TMessage);
        }
        public object GetInnerHandler()
        {
            return _messageHandler;
        }
    }
}
