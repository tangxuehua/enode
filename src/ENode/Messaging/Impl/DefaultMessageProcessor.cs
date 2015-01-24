using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Messaging.Impl
{
    public class DefaultMessageProcessor : AbstractParallelProcessor<IMessage>
    {
        #region Private Variables

        private readonly IDispatcher<IMessage> _messageDispatcher;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultMessageProcessor(IDispatcher<IMessage> messageDispatcher, ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.MessageProcessorParallelThreadCount, "ProcessMessage")
        {
            _messageDispatcher = messageDispatcher;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected override QueueMessage<IMessage> CreateQueueMessage(IMessage message, IProcessContext<IMessage> processContext)
        {
            var hashKey = message is IVersionedMessage ? ((IVersionedMessage)message).SourceId : message.Id;
            return new QueueMessage<IMessage>(hashKey, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<IMessage> queueMessage)
        {
            var message = queueMessage.Payload;
            var success = _messageDispatcher.DispatchMessage(message);

            if (!success)
            {
                _logger.ErrorFormat("Process message failed, messageId:{0}, messageType:{1}, retryTimes:{2}", message.Id, message.GetType().Name, queueMessage.RetryTimes);
            }

            OnMessageHandled(!success, queueMessage);
        }
    }
}
