using System.Threading.Tasks;
using ECommon.Logging;
using ECommon.IO;

namespace ENode.Infrastructure.Impl
{
    public abstract class AbstractSequenceProcessingMessageHandler<X, Y> : IProcessingMessageHandler<X, Y>
        where X : class, IProcessingMessage<X, Y>, ISequenceProcessingMessage
        where Y : ISequenceMessage
    {
        #region Private Variables

        private readonly IPublishedVersionStore _publishedVersionStore;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        public abstract string Name { get; }

        #region Constructors

        public AbstractSequenceProcessingMessageHandler(IPublishedVersionStore publishedVersionStore, IOHelper ioHelper, ILoggerFactory loggerFactory)
        {
            _publishedVersionStore = publishedVersionStore;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected abstract Task<AsyncTaskResult> DispatchProcessingMessageAsync(X processingMessage);

        public void HandleAsync(X processingMessage)
        {
            HandleMessageAsync(processingMessage, 0);
        }

        private void HandleMessageAsync(X processingMessage, int retryTimes)
        {
            var message = processingMessage.Message;

            _ioHelper.TryAsyncActionRecursively("GetPublishedVersionAsync",
            () => _publishedVersionStore.GetPublishedVersionAsync(Name, message.AggregateRootTypeName, message.AggregateRootStringId),
            currentRetryTimes => HandleMessageAsync(processingMessage, currentRetryTimes),
            result =>
            {
                var publishedVersion = result.Data;
                if (publishedVersion + 1 == message.Version)
                {
                    DispatchProcessingMessageAsync(processingMessage, 0);
                }
                else if (publishedVersion + 1 < message.Version)
                {
                    _logger.InfoFormat("The sequence message cannot be process now as the version is not the next version, it will be handle later. contextInfo [aggregateRootId={0},lastPublishedVersion={1},messageVersion={2}]", message.AggregateRootStringId, publishedVersion, message.Version);
                    processingMessage.AddToWaitingList();
                }
                else
                {
                    processingMessage.Complete();
                }
            },
            () => string.Format("sequence message [messageId:{0}, messageType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}]", message.Id, message.GetType().Name, message.AggregateRootStringId, message.Version),
            errorMessage =>
            {
                _logger.Fatal(string.Format("Get published version has unknown exception, the code should not be run to here, errorMessage: {0}", errorMessage));
            },
            retryTimes, true);
        }
        private void DispatchProcessingMessageAsync(X processingMessage, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("DispatchProcessingMessageAsync",
            () => DispatchProcessingMessageAsync(processingMessage),
            currentRetryTimes => DispatchProcessingMessageAsync(processingMessage, currentRetryTimes),
            result =>
            {
                UpdatePublishedVersionAsync(processingMessage, 0);
            },
            () => string.Format("sequence message [messageId:{0}, messageType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}]", processingMessage.Message.Id, processingMessage.Message.GetType().Name, processingMessage.Message.AggregateRootStringId, processingMessage.Message.Version),
            errorMessage =>
            {
                _logger.Fatal(string.Format("Dispatching message has unknown exception, the code should not be run to here, errorMessage: {0}", errorMessage));
            },
            retryTimes, true);
        }
        private void UpdatePublishedVersionAsync(X processingMessage, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("UpdatePublishedVersionAsync",
            () => _publishedVersionStore.UpdatePublishedVersionAsync(Name, processingMessage.Message.AggregateRootTypeName, processingMessage.Message.AggregateRootStringId, processingMessage.Message.Version),
            currentRetryTimes => UpdatePublishedVersionAsync(processingMessage, currentRetryTimes),
            result =>
            {
                processingMessage.Complete();
            },
            () => string.Format("sequence message [messageId:{0}, messageType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}]", processingMessage.Message.Id, processingMessage.Message.GetType().Name, processingMessage.Message.AggregateRootStringId, processingMessage.Message.Version),
            errorMessage =>
            {
                _logger.Fatal(string.Format("Update published version has unknown exception, the code should not be run to here, errorMessage: {0}", errorMessage));
            },
            retryTimes, true);
        }
    }
}
