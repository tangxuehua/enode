using System.Collections.Concurrent;

namespace ENode.Infrastructure
{
    public class DefaultMessageProcessor<X, Y, Z> : IMessageProcessor<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        private readonly ConcurrentDictionary<string, ProcessingMessageMailbox<X, Y, Z>> _mailboxDict;
        private readonly IProcessingMessageScheduler<X, Y, Z> _processingMessageScheduler;
        private readonly IProcessingMessageHandler<X, Y, Z> _processingMessageHandler;

        public DefaultMessageProcessor(IProcessingMessageScheduler<X, Y, Z> processingMessageScheduler, IProcessingMessageHandler<X, Y, Z> processingMessageHandler)
        {
            _mailboxDict = new ConcurrentDictionary<string, ProcessingMessageMailbox<X, Y, Z>>();
            _processingMessageScheduler = processingMessageScheduler;
            _processingMessageHandler = processingMessageHandler;
        }

        public void Process(X processingMessage)
        {
            var routingKey = processingMessage.Message.GetRoutingKey();
            if (!string.IsNullOrWhiteSpace(routingKey))
            {
                var mailbox = _mailboxDict.GetOrAdd(routingKey, x =>
                {
                    return new ProcessingMessageMailbox<X, Y, Z>(_processingMessageScheduler, _processingMessageHandler);
                });
                mailbox.EnqueueMessage(processingMessage);
                _processingMessageScheduler.ScheduleMailbox(mailbox);
            }
            else
            {
                _processingMessageScheduler.ScheduleMessage(processingMessage);
            }
        }
    }
}
