using System.Threading.Tasks;
using ECommon.IO;
using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DomainEventStreamMessageHandler : AbstractSequenceProcessingMessageHandler<ProcessingDomainEventStreamMessage, DomainEventStreamMessage>
    {
        private readonly IMessageDispatcher _dispatcher;

        public DomainEventStreamMessageHandler(IPublishedVersionStore publishedVersionStore, IMessageDispatcher dispatcher, IOHelper ioHelper, ILoggerFactory loggerFactory)
            : base(publishedVersionStore, ioHelper, loggerFactory)
        {
            _dispatcher = dispatcher;
        }

        public override string Name
        {
            get { return ENodeConfiguration.Instance.Setting.DomainEventStreamMessageHandlerName; }
        }

        protected override Task<AsyncTaskResult> DispatchProcessingMessageAsync(ProcessingDomainEventStreamMessage processingMessage)
        {
            return _dispatcher.DispatchMessagesAsync(processingMessage.Message.Events);
        }
    }
}
