using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionProcessor : AbstractParallelProcessor<IPublishableException>
    {
        #region Private Variables

        private readonly IDispatcher<IPublishableException> _exceptionDispatcher;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultExceptionProcessor(IDispatcher<IPublishableException> exceptionDispatcher, ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.ExceptionProcessorParallelThreadCount, "ProcessPublishableException")
        {
            _exceptionDispatcher = exceptionDispatcher;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected override QueueMessage<IPublishableException> CreateQueueMessage(IPublishableException message, IProcessContext<IPublishableException> processContext)
        {
            return new QueueMessage<IPublishableException>(message.Id, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<IPublishableException> queueMessage)
        {
            var publishableException = queueMessage.Payload;
            var success = _exceptionDispatcher.DispatchMessage(publishableException);

            if (!success)
            {
                _logger.ErrorFormat("Process publishable exception failed, exceptionId:{0}, exceptionType:{1}, retryTimes:{2}", publishableException.Id, publishableException.GetType().Name, queueMessage.RetryTimes);
            }

            OnMessageHandled(success, queueMessage);
        }
    }
}
