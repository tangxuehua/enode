using System.Collections.Concurrent;
using ECommon.Logging;

namespace ENode.Infrastructure
{
    public class DefaultMessageProcessor<X, Y> : IMessageProcessor<X, Y>
        where X : class, IProcessingMessage<X, Y>
        where Y : IMessage
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ProcessingMessageMailbox<X, Y>> _mailboxDict;
        private readonly IProcessingMessageScheduler<X, Y> _processingMessageScheduler;
        private readonly IProcessingMessageHandler<X, Y> _processingMessageHandler;

        public DefaultMessageProcessor(IProcessingMessageScheduler<X, Y> processingMessageScheduler, IProcessingMessageHandler<X, Y> processingMessageHandler, ILoggerFactory loggerFactory)
        {
            _mailboxDict = new ConcurrentDictionary<string, ProcessingMessageMailbox<X, Y>>();
            _processingMessageScheduler = processingMessageScheduler;
            _processingMessageHandler = processingMessageHandler;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public void Process(X processingMessage)
        {
            var routingKey = processingMessage.Message.GetRoutingKey();
            if (!string.IsNullOrWhiteSpace(routingKey))
            {
                var mailbox = _mailboxDict.GetOrAdd(routingKey, x =>
                {
                    return new ProcessingMessageMailbox<X, Y>(routingKey, _processingMessageScheduler, _processingMessageHandler, _logger);
                });
                mailbox.EnqueueMessage(processingMessage);
            }
            else
            {
                _processingMessageScheduler.ScheduleMessage(processingMessage);
            }
        }
    }
}
