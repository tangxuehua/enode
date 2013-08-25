using System;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Concurrent;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Retring;
using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IUncommittedEventExecutor.
    /// </summary>
    public class DefaultUncommittedEventExecutor : MessageExecutor<EventStream>, IUncommittedEventExecutor
    {
        #region Private Variables

        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly ICommandAsyncResultManager _commandAsyncResultManager;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IRepository _repository;
        private readonly IRetryCommandService _retryCommandService;
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRetryService _retryService;
        private readonly IEventPersistenceSynchronizerProvider _eventPersistenceSynchronizerProvider;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processingCommandCache"></param>
        /// <param name="commandAsyncResultManager"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="memoryCache"></param>
        /// <param name="repository"></param>
        /// <param name="retryCommandService"></param>
        /// <param name="eventStore"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="retryService"></param>
        /// <param name="eventPersistenceSynchronizerProvider"></param>
        /// <param name="loggerFactory"></param>
        public DefaultUncommittedEventExecutor(
            IProcessingCommandCache processingCommandCache,
            ICommandAsyncResultManager commandAsyncResultManager,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IAggregateRootFactory aggregateRootFactory,
            IMemoryCache memoryCache,
            IRepository repository,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            IRetryService retryService,
            IEventPersistenceSynchronizerProvider eventPersistenceSynchronizerProvider,
            ILoggerFactory loggerFactory)
        {
            _processingCommandCache = processingCommandCache;
            _commandAsyncResultManager = commandAsyncResultManager;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _memoryCache = memoryCache;
            _repository = repository;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _eventPublisher = eventPublisher;
            _retryService = retryService;
            _eventPersistenceSynchronizerProvider = eventPersistenceSynchronizerProvider;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        /// <summary>Execute the given event stream.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        public override void Execute(EventStream message, IMessageQueue<EventStream> queue)
        {
            var eventStreamContext = new EventStreamContext { EventStream = message, Queue = queue };

            Func<EventStreamContext, bool> tryCommitEventsAction = (context) =>
            {
                try
                {
                    return CommitEvents(context);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when committing events:{0}.", context.EventStream.GetStreamInformation()), ex);
                    return false;
                }
            };

            if (!_retryService.TryAction("TryCommitEvents", () => tryCommitEventsAction(eventStreamContext), 3))
            {
                _retryService.RetryInQueue(new ActionInfo("TryCommitEvents", obj => tryCommitEventsAction(obj as EventStreamContext), eventStreamContext, null));
            }
        }

        #region Private Methods

        private bool CommitEvents(EventStreamContext eventStreamContext)
        {
            var errorInfo = new ErrorInfo();
            var synchronizeResult = TryCallSynchronizersBeforeEventPersisting(eventStreamContext.EventStream, errorInfo);

            switch (synchronizeResult)
            {
                case SynchronizeResult.SynchronizerConcurrentException:
                    return false;
                case SynchronizeResult.Failed:
                    Clear(eventStreamContext, errorInfo);
                    return true;
                default:
                {
                    var persistSuccessActionInfo = new ActionInfo(
                        "PersistSuccessAction",
                        obj =>
                        {
                            var context = obj as EventStreamContext;
                            if (context == null)
                            {
                                throw new Exception("Invalid event stream context.");
                            }
                            if (context.HasConcurrentException)
                            {
                                var retryCommandSuccessAction = new ActionInfo(
                                    "RetryCommandSuccessAction",
                                    data =>
                                    {
                                        var streamContext = data as EventStreamContext;
                                        if (streamContext == null)
                                        {
                                            throw new Exception("Invalid event stream context.");
                                        }
                                        FinishExecution(streamContext.EventStream, streamContext.Queue);
                                        return true;
                                    },
                                    context, null);
                                RetryCommand(context, context.ErrorInfo, retryCommandSuccessAction);
                            }
                            else
                            {
                                TryRefreshMemoryCache(context.EventStream);
                                TryCallSynchronizersAfterEventPersisted(context.EventStream);
                                TryPublishEvents(context.EventStream, new ActionInfo("PublishSuccessAction", (data) => { Clear(data as EventStreamContext); return true; }, context, null));
                            }
                            return true;
                        },
                        eventStreamContext,
                        null);

                    TryPersistEvents(eventStreamContext, persistSuccessActionInfo);

                    return true;
                }
            }
        }
        private bool IsEventStreamCommitted(EventStream eventStream)
        {
            return _eventStore.IsEventStreamExist(
                eventStream.AggregateRootId,
                _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName),
                eventStream.Id);
        }
        private void TryPersistEvents(EventStreamContext eventStreamContext, ActionInfo successActionInfo)
        {
            Func<EventStreamContext, bool> tryPersistEventsAction = (context) =>
            {
                try
                {
                    _eventStore.Append(context.EventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex is ConcurrentException && IsEventStreamCommitted(context.EventStream))
                    {
                        return true;
                    }

                    var errorMessage = string.Format("{0} raised when persisting events:{1}", ex.GetType().Name, context.EventStream.GetStreamInformation());
                    _logger.Error(errorMessage, ex);

                    if (ex is ConcurrentException)
                    {
                        context.SetConcurrentException(new ErrorInfo { ErrorMessage = errorMessage, Exception = ex });
                        return true;
                    }

                    return false;
                }
            };

            if (_retryService.TryAction("TryPersistEvents", () => tryPersistEventsAction(eventStreamContext), 3))
            {
                successActionInfo.Action(successActionInfo.Data);
            }
            else
            {
                _retryService.RetryInQueue(new ActionInfo("TryPersistEvents", obj => tryPersistEventsAction(obj as EventStreamContext), eventStreamContext, successActionInfo));
            }
        }
        private void TryRefreshMemoryCache(EventStream eventStream)
        {
            try
            {
                RefreshMemoryCache(eventStream);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache by event stream:{0}", eventStream.GetStreamInformation()), ex);
            }
        }
        private void RefreshMemoryCache(EventStream eventStream)
        {
            var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName);

            if (aggregateRootType == null)
            {
                throw new Exception(string.Format("Could not find aggregate root type by aggregate root name {0}", eventStream.AggregateRootName));
            }

            if (eventStream.Version == 1)
            {
                var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                aggregateRoot.ReplayEventStream(eventStream);
                _memoryCache.Set(aggregateRoot);
            }
            else if (eventStream.Version > 1)
            {
                var aggregateRoot = _memoryCache.Get(eventStream.AggregateRootId);
                if (aggregateRoot == null)
                {
                    aggregateRoot = _repository.Get(aggregateRootType, eventStream.AggregateRootId);
                    if (aggregateRoot != null)
                    {
                        _memoryCache.Set(aggregateRoot);
                    }
                }
                else if (aggregateRoot.Version + 1 == eventStream.Version)
                {
                    aggregateRoot.ReplayEventStream(eventStream);
                    _memoryCache.Set(aggregateRoot);
                }
                else if (aggregateRoot.Version + 1 < eventStream.Version)
                {
                    aggregateRoot = _repository.Get(aggregateRootType, eventStream.AggregateRootId);
                    if (aggregateRoot != null)
                    {
                        _memoryCache.Set(aggregateRoot);
                    }
                }
            }
        }
        private void TryPublishEvents(EventStream eventStream, ActionInfo successActionInfo)
        {
            Func<EventStream, bool> tryPublishEventsAction = (stream) =>
            {
                try
                {
                    _eventPublisher.Publish(stream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", stream.GetStreamInformation()), ex);
                    return false;
                }
            };

            if (_retryService.TryAction("TryPublishEvents", () => tryPublishEventsAction(eventStream), 3))
            {
                successActionInfo.Action(successActionInfo.Data);
            }
            else
            {
                _retryService.RetryInQueue(new ActionInfo("TryPublishEvents", (obj) => tryPublishEventsAction(obj as EventStream), eventStream, successActionInfo));
            }
        }
        private SynchronizeResult TryCallSynchronizersBeforeEventPersisting(EventStream eventStream, ErrorInfo errorInfo)
        {
            foreach (var evnt in eventStream.Events)
            {
                var synchronizers = _eventPersistenceSynchronizerProvider.GetSynchronizers(evnt.GetType());
                foreach (var synchronizer in synchronizers)
                {
                    try
                    {
                        synchronizer.OnBeforePersisting(evnt);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format(
                            "Exception raised when calling synchronizer's OnBeforePersisting method. synchronizer:{0}, events:{1}",
                            synchronizer.GetInnerSynchronizer().GetType().Name,
                            eventStream.GetStreamInformation());
                        _logger.Error(errorMessage, ex);
                        errorInfo.ErrorMessage = errorMessage;
                        errorInfo.Exception = ex;
                        if (ex is ConcurrentException)
                        {
                            return SynchronizeResult.SynchronizerConcurrentException;
                        }
                        return SynchronizeResult.Failed;
                    }
                }
            }

            return SynchronizeResult.Success;
        }
        private void TryCallSynchronizersAfterEventPersisted(EventStream eventStream)
        {
            foreach (var evnt in eventStream.Events)
            {
                var synchronizers = _eventPersistenceSynchronizerProvider.GetSynchronizers(evnt.GetType());
                foreach (var synchronizer in synchronizers)
                {
                    try
                    {
                        synchronizer.OnAfterPersisted(evnt);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format(
                            "Exception raised when calling synchronizer's OnAfterPersisted method. synchronizer:{0}, events:{1}",
                            synchronizer.GetInnerSynchronizer().GetType().Name,
                            eventStream.GetStreamInformation()), ex);
                    }
                }
            }
        }
        private void RetryCommand(EventStreamContext eventStreamContext, ErrorInfo errorInfo, ActionInfo successActionInfo)
        {
            if (!eventStreamContext.EventStream.IsRestoreFromStorage())
            {
                var commandInfo = _processingCommandCache.Get(eventStreamContext.EventStream.CommandId);
                if (commandInfo != null)
                {
                    _retryCommandService.RetryCommand(commandInfo, eventStreamContext.EventStream, errorInfo, successActionInfo);
                }
                else
                {
                    _logger.ErrorFormat("The command need to retry cannot be found from command processing cache, commandId:{0}", eventStreamContext.EventStream.CommandId);
                }
            }
            else
            {
                _logger.InfoFormat("The command with id {0} will not be retry as the current event stream is restored from the message store.", eventStreamContext.EventStream.CommandId);
            }
        }

        private void Clear(EventStreamContext context, ErrorInfo errorInfo = null)
        {
            if (errorInfo != null)
            {
                _commandAsyncResultManager.TryComplete(context.EventStream.CommandId, context.EventStream.AggregateRootId, errorInfo.ErrorMessage, errorInfo.Exception);
            }
            else
            {
                _commandAsyncResultManager.TryComplete(context.EventStream.CommandId, context.EventStream.AggregateRootId);
            }
            _processingCommandCache.TryRemove(context.EventStream.CommandId);
            FinishExecution(context.EventStream, context.Queue);
        }

        #endregion

        enum SynchronizeResult
        {
            Success,
            SynchronizerConcurrentException,
            Failed
        }
    }
}
