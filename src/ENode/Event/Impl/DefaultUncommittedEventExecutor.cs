using System;
using System.Linq;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Messaging;

namespace ENode.Eventing
{
    public class DefaultUncommittedEventExecutor : MessageExecutor<EventStream>, IUncommittedEventExecutor
    {
        #region Private Variables

        private IProcessingCommandCache _processingCommandCache;
        private ICommandAsyncResultManager _commandAsyncResultManager;
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private IAggregateRootFactory _aggregateRootFactory;
        private IMemoryCache _memoryCache;
        private IRepository _repository;
        private IRetryCommandService _retryCommandService;
        private IEventStore _eventStore;
        private IEventPublisher _eventPublisher;
        private IRetryService _retryService;
        private IEventPersistenceSynchronizerProvider _eventPersistenceSynchronizerProvider;
        private ILogger _logger;

        #endregion

        #region Constructors

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
            ICommandContext commandContext,
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
                _retryService.RetryInQueue(new ActionInfo("TryCommitEvents", (obj) => tryCommitEventsAction(obj as EventStreamContext), eventStreamContext, null));
            }
        }

        #region Private Methods

        private bool CommitEvents(EventStreamContext eventStreamContext)
        {
            var synchronizeResult = TryCallSynchronizersBeforeEventPersisting(eventStreamContext.EventStream);

            if (synchronizeResult == SynchronizeResult.SynchronizerConcurrentException)
            {
                return false;
            }
            else if (synchronizeResult == SynchronizeResult.Failed)
            {
                Clear(eventStreamContext);
                return true;
            }
            else
            {
                var persistSuccessActionInfo = new ActionInfo(
                    "PersistSuccessAction",
                    (obj) =>
                    {
                        var context = obj as EventStreamContext;
                        if (context.HasConcurrentException)
                        {
                            var retryCommandSuccessAction = new ActionInfo(
                                "RetryCommandSuccessAction",
                                (data) =>
                                {
                                    var streamContext = data as EventStreamContext;
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
        private bool IsEventStreamCommitted(EventStream eventStream)
        {
            return _eventStore.IsEventStreamExist(
                eventStream.AggregateRootId,
                _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName),
                eventStream.Id,
                eventStream.CommandId);
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
                _retryService.RetryInQueue(new ActionInfo("TryPersistEvents", (obj) => tryPersistEventsAction(obj as EventStreamContext), eventStreamContext, successActionInfo));
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
        private SynchronizeResult TryCallSynchronizersBeforeEventPersisting(EventStream eventStream)
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
                var command = _processingCommandCache.Get(eventStreamContext.EventStream.CommandId);
                if (command != null)
                {
                    _retryCommandService.RetryCommand(command, errorInfo, successActionInfo);
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
        private void Clear(EventStreamContext context)
        {
            _commandAsyncResultManager.TryComplete(context.EventStream.CommandId);
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
