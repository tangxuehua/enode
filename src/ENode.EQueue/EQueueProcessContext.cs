using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EQueueProcessContext : IMessageProcessContext
    {
        protected readonly QueueMessage _queueMessage;
        protected readonly IMessageContext _messageContext;

        public EQueueProcessContext(QueueMessage queueMessage, IMessageContext messageContext)
        {
            _queueMessage = queueMessage;
            _messageContext = messageContext;
        }

        public virtual void NotifyMessageProcessed()
        {
            _messageContext.OnMessageHandled(_queueMessage);
        }
    }
}
