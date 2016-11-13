using ECommon.Logging;

namespace ENode.Infrastructure.Impl
{
    public class DefaultPublishableExceptionProcessor : DefaultMessageProcessor<ProcessingPublishableExceptionMessage, IPublishableException>
    {
        public DefaultPublishableExceptionProcessor(
            IProcessingMessageScheduler<ProcessingPublishableExceptionMessage, IPublishableException> processingMessageScheduler,
            IProcessingMessageHandler<ProcessingPublishableExceptionMessage, IPublishableException> processingMessageHandler,
            ILoggerFactory loggerFactory) : base(processingMessageScheduler, processingMessageHandler, loggerFactory)
        {
        }

        public override string MessageName
        {
            get
            {
                return "exception message";
            }
        }
    }
}
