using ECommon.Logging;
using ENode.Eventing;

namespace ENode.Infrastructure.Impl
{
    public class DefaultDomainEventProcessor : DefaultMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage>
    {
        public DefaultDomainEventProcessor(
            IProcessingMessageScheduler<ProcessingDomainEventStreamMessage, DomainEventStreamMessage> processingMessageScheduler,
            IProcessingMessageHandler<ProcessingDomainEventStreamMessage, DomainEventStreamMessage> processingMessageHandler,
            ILoggerFactory loggerFactory) : base(processingMessageScheduler, processingMessageHandler, loggerFactory)
        {
        }

        public override string MessageName
        {
            get
            {
                return "event message";
            }
        }
    }
}
