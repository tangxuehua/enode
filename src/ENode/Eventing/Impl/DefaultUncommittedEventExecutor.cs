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
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IMemoryCache _memoryCache;
        private readonly IRepository _repository;
        private readonly IRetryCommandService _retryCommandService;
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly IEventSynchronizerProvider _eventSynchronizerProvider;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processingCommandCache"></param>
        /// <param name="commandAsyncResultManager"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="eventSourcingService"></param>
        /// <param name="memoryCache"></param>
        /// <param name="repository"></param>
        /// <param name="retryCommandService"></param>
        /// <param name="eventStore"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="eventSynchronizerProvider"></param>
        /// <param name="loggerFactory"></param>
        public DefaultUncommittedEventExecutor(
            IProcessingCommandCache processingCommandCache,
            ICommandAsyncResultManager commandAsyncResultManager,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IAggregateRootFactory aggregateRootFactory,
            IEventSourcingService eventSourcingService,
            IMemoryCache memoryCache,
            IRepository repository,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            IActionExecutionService actionExecutionService,
            IEventSynchronizerProvider eventSynchronizerProvider,
            ILoggerFactory loggerFactory)
        {
            _processingCommandCache = processingCommandCache;
            _commandAsyncResultManager = commandAsyncResultManager;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _eventSourcingService = eventSourcingService;
            _memoryCache = memoryCache;
            _repository = repository;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _eventPublisher = eventPublisher;
            _actionExecutionService = actionExecutionService;
            _eventSynchronizerProvider = eventSynchronizerProvider;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        /// <summary>Execute the given event stream.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        public override void Execute(EventStream message, IMessageQueue<EventStream> queue)
        {
            var context = new EventStreamContext { EventStream = message, Queue = queue };

            Func<bool> commitEvents = () =>
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

            _actionExecutionService.TryAction("CommitEvents", commitEvents, 3, null);
        }

        #region Private Methods

        private bool CommitEvents(EventStreamContext context)
        {
            var synchronizeResult = SyncBeforeEventPersisting(context.EventStream);

            switch (synchronizeResult.Status)
            {
                case SynchronizeStatus.SynchronizerConcurrentException:
                    return false;
                case SynchronizeStatus.Failed:
                    CleanEvents(context, synchronizeResult.ErrorInfo);
                    return true;
                default:
                {
                    Func<object, bool> persistEventsCallback = (obj) =>
                    {
                        if (context.HasConcurrentException)
                        {
                            RefreshMemoryCache(context.EventStream);
                            RetryCommand(context, context.ErrorInfo, new ActionInfo("RetryCommandCallback", data => { context.Queue.Delete(context.EventStream); return true; }, null, null));
                        }
                        else
                        {
                            RefreshMemoryCache(context.EventStream);
                            SyncAfterEventPersisted(context.EventStream);
                            PublishEvents(context.EventStream, new ActionInfo("PublishEventsCallback", data => { CleanEvents(context); return true; }, null, null));
                        }
                        return true;
                    };

                    PersistEvents(context, new ActionInfo("PersistEventsCallback", persistEventsCallback, null, null));

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
        private void PersistEvents(EventStreamContext context, ActionInfo successCallback)
        {
            Func<bool> persistEvents = () =>
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
                        context.SetConcurrentException(new ErrorInfo(errorMessage, ex));
                        return true;
                    }

                    return false;
                }
            };

            _actionExecutionService.TryAction("PersistEvents", persistEvents, 3, successCallback);
        }
        private void RefreshMemoryCache(EventStream eventStream)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName);

                if (aggregateRootType == null)
                {
                    throw new Exception(string.Format("Could not find aggregate root type by aggregate root name {0}", eventStream.AggregateRootName));
                }

                if (eventStream.Version == 1)
                {
                    var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                    _eventSourcingService.ReplayEventStream(aggregateRoot, new EventStream[] { eventStream });
                    _memoryCache.Set(aggregateRoot);
                }
                else if (eventStream.Version > 1)
                {
                    var aggregateRoot = _memoryCache.Get(eventStream.AggregateRootId, aggregateRootType);
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
                        _eventSourcingService.ReplayEventStream(aggregateRoot, new EventStream[] { eventStream });
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
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache by event stream:{0}", eventStream.GetStreamInformation()), ex);
            }
        }
        private void PublishEvents(EventStream eventStream, ActionInfo successCallback)
        {
            Func<bool> publishEvents = () =>
            {
                try
                {
                    _eventPublisher.Publish(eventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream.GetStreamInformation()), ex);
                    return false;
                }
            };

            _actionExecutionService.TryAction("PublishEvents", publishEvents, 3, successCallback);
        }
        private SynchronizeResult SyncBeforeEventPersisting(EventStream eventStream)
        {
            var result = new SynchronizeResult {Status = SynchronizeStatus.Success};

            foreach (var evnt in eventStream.Events)
            {
                var synchronizers = _eventSynchronizerProvider.GetSynchronizers(evnt.GetType());
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
                        result.ErrorInfo = new ErrorInfo(errorMessage, ex);
                        if (ex is ConcurrentException)
                        {
                            result.Status = SynchronizeStatus.SynchronizerConcurrentException;
                            return result;
                        }
                        result.Status = SynchronizeStatus.Failed;
                        return result;
                    }
                }
            }

            return result;
        }
        private void SyncAfterEventPersisted(EventStream eventStream)
        {
            foreach (var evnt in eventStream.Events)
            {
                var synchronizers = _eventSynchronizerProvider.GetSynchronizers(evnt.GetType());
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
        private void RetryCommand(EventStreamContext context, ErrorInfo errorInfo, ActionInfo successCallback)
        {
            Func<bool> retryCommand = () =>
            {
                var eventStream = context.EventStream;
                if (!eventStream.IsRestoreFromStorage())
                {
                    var commandInfo = _processingCommandCache.Get(eventStream.CommandId);
                    if (commandInfo != null)
                    {
                        _retryCommandService.RetryCommand(commandInfo, eventStream, errorInfo);
                    }
                    else
                    {
                        _logger.ErrorFormat("The command need to retry cannot be found from command processing cache, commandId:{0}", eventStream.CommandId);
                    }
                }
                else
                {
                    _logger.InfoFormat("The command with id {0} will not be retry as the current event stream is restored from the message store.", eventStream.CommandId);
                }
                return true;
            };
            _actionExecutionService.TryAction("RetryCommand", retryCommand, 3, successCallback);
        }
        private void CleanEvents(EventStreamContext context, ErrorInfo errorInfo = null)
        {
            _commandAsyncResultManager.TryComplete(context.EventStream.CommandId, context.EventStream.AggregateRootId, errorInfo);
            _processingCommandCache.TryRemove(context.EventStream.CommandId);
            context.Queue.Delete(context.EventStream);
        }

        #endregion

        class SynchronizeResult
        {
            public SynchronizeStatus Status { get; set; }
            public ErrorInfo ErrorInfo { get; set; }
        }
        enum SynchronizeStatus
        {
            Success,
            SynchronizerConcurrentException,
            Failed
        }
    }
}
