using ECommon.Logging;

namespace ENode.Infrastructure.Impl
{
    public class DefaultApplicationMessageProcessor : DefaultMessageProcessor<ProcessingApplicationMessage, IApplicationMessage>
    {
        public DefaultApplicationMessageProcessor(
            IProcessingMessageScheduler<ProcessingApplicationMessage, IApplicationMessage> processingMessageScheduler,
            IProcessingMessageHandler<ProcessingApplicationMessage, IApplicationMessage> processingMessageHandler,
            ILoggerFactory loggerFactory) : base(processingMessageScheduler, processingMessageHandler, loggerFactory)
        {
        }

        public override string MessageName
        {
            get
            {
                return "application message";
            }
        }
    }
}
