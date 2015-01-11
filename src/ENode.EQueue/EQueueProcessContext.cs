using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EQueueProcessContext<TMessage> : IProcessContext<TMessage> where TMessage : class
    {
        protected readonly QueueMessage _queueMessage;
        protected readonly IMessageContext _messageContext;
        protected readonly TMessage _message;

        public EQueueProcessContext(QueueMessage queueMessage, IMessageContext messageContext, TMessage message)
        {
            _queueMessage = queueMessage;
            _messageContext = messageContext;
            _message = message;
        }

        public virtual void OnProcessed(TMessage message)
        {
            _messageContext.OnMessageHandled(_queueMessage);
        }
    }
}
