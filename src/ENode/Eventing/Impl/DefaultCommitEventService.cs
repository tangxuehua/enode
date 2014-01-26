using System;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of ICommitService.
    /// </summary>
    public class DefaultCommitEventService : ICommitEventService
    {
        #region Private Variables

        private readonly ICommandCompletionEventManager _commandCompletionEventManager;
        private readonly IWaitingCommandService _waitingCommandService;
        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly IRetryCommandService _retryCommandService;
        private readonly IEventStore _eventStore;
        private readonly IPublishEventService _publishEventService;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly IEventSynchronizerProvider _eventSynchronizerProvider;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandCompletionEventManager"></param>
        /// <param name="waitingCommandService"></param>
        /// <param name="processingCommandCache"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="eventSourcingService"></param>
        /// <param name="memoryCache"></param>
        /// <param name="aggregateStorage"></param>
        /// <param name="retryCommandService"></param>
        /// <param name="eventStore"></param>
        /// <param name="publishEventService"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="eventSynchronizerProvider"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommitEventService(
            ICommandCompletionEventManager commandCompletionEventManager,
            IWaitingCommandService waitingCommandService,
            IProcessingCommandCache processingCommandCache,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IAggregateRootFactory aggregateRootFactory,
            IEventSourcingService eventSourcingService,
            IMemoryCache memoryCache,
            IAggregateStorage aggregateStorage,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IPublishEventService publishEventService,
            IActionExecutionService actionExecutionService,
            IEventSynchronizerProvider eventSynchronizerProvider,
            ILoggerFactory loggerFactory)
        {
            _commandCompletionEventManager = commandCompletionEventManager;
            _waitingCommandService = waitingCommandService;
            _processingCommandCache = processingCommandCache;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _eventSourcingService = eventSourcingService;
            _memoryCache = memoryCache;
            _aggregateStorage = aggregateStorage;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _publishEventService = publishEventService;
            _actionExecutionService = actionExecutionService;
            _eventSynchronizerProvider = eventSynchronizerProvider;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        /// <summary>Commit the domain events to the eventstore and publish the domain events.
        /// </summary>
        /// <param name="context"></param>
        public void CommitEvent(EventCommittingContext context)
        {
            var commitEvents = new Func<bool>(() =>
            {
                try
                {
                    return CommitEvents(context);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when committing events:{0}.", context.EventStream), ex);
                    return false;
                }
            });

            _actionExecutionService.TryAction("CommitEvents", commitEvents, 3, null);
        }

        #region Private Methods

        private bool CommitEvents(EventCommittingContext context)
        {
            var synchronizeStatus = SyncBeforeEventPersisting(context.EventStream);

            switch (synchronizeStatus)
            {
                case SynchronizeStatus.SynchronizerConcurrentException:
                    return false;
                case SynchronizeStatus.Failed:
                    CompleteCommandExecution(context);
                    return true;
                default:
                {
                    var persistEventsCallback = new Func<object, bool>(obj =>
                    {
                        var currentContext = obj as EventCommittingContext;
                        var eventStream = currentContext.EventStream;

                        if (currentContext.ConcurrentException != null)
                        {
                            RefreshMemoryCacheFromEventStore(eventStream);
                            RetryCommand(currentContext);
                        }
                        else
                        {
                            RefreshMemoryCache(eventStream);
                            SendWaitingCommand(eventStream);
                            SyncAfterEventPersisted(eventStream);
                            PublishEvents(currentContext);
                        }
                        return true;
                    });

                    PersistEvents(context, new ActionInfo("PersistEventsCallback", persistEventsCallback, context, null));

                    return true;
                }
            }
        }
        private void PersistEvents(EventCommittingContext context, ActionInfo successCallback)
        {
            var persistEvents = new Func<bool>(() =>
            {
                try
                {
                    _eventStore.Append(context.EventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{0} raised when persisting events:{1}", ex.GetType().Name, context.EventStream), ex);
                    if (ex is ConcurrentException)
                    {
                        context.ConcurrentException = ex as ConcurrentException;
                        return true;
                    }
                    return false;
                }
            });

            _actionExecutionService.TryAction("PersistEvents", persistEvents, 3, successCallback);
        }
        private void SendWaitingCommand(EventStream eventStream)
        {
            _waitingCommandService.SendWaitingCommand(eventStream.AggregateRootId);
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
                    _eventSourcingService.ReplayEvents(aggregateRoot, new EventStream[] { eventStream });
                    _memoryCache.Set(aggregateRoot);
                }
                else if (eventStream.Version > 1)
                {
                    var aggregateRoot = _memoryCache.Get(eventStream.AggregateRootId, aggregateRootType);
                    if (aggregateRoot == null)
                    {
                        aggregateRoot = _aggregateStorage.Get(aggregateRootType, eventStream.AggregateRootId);
                        if (aggregateRoot != null)
                        {
                            _memoryCache.Set(aggregateRoot);
                        }
                    }
                    else if (aggregateRoot.Version + 1 == eventStream.Version)
                    {
                        _eventSourcingService.ReplayEvents(aggregateRoot, new EventStream[] { eventStream });
                        _memoryCache.Set(aggregateRoot);
                    }
                    else if (aggregateRoot.Version + 1 < eventStream.Version)
                    {
                        aggregateRoot = _aggregateStorage.Get(aggregateRootType, eventStream.AggregateRootId);
                        if (aggregateRoot != null)
                        {
                            _memoryCache.Set(aggregateRoot);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache by event stream:{0}", eventStream), ex);
            }
        }
        private void RefreshMemoryCacheFromEventStore(EventStream eventStream)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName);
                if (aggregateRootType == null)
                {
                    throw new Exception(string.Format("Could not find aggregate root type by aggregate root name {0}", eventStream.AggregateRootName));
                }
                var aggregateRoot = _aggregateStorage.Get(aggregateRootType, eventStream.AggregateRootId);
                if (aggregateRoot != null)
                {
                    _memoryCache.Set(aggregateRoot);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache by event stream:{0}", eventStream), ex);
            }
        }
        private void PublishEvents(EventCommittingContext context)
        {
            _publishEventService.PublishEvent(context.EventStream, context.Command, context.CommandExecuteContext);
        }
        private SynchronizeStatus SyncBeforeEventPersisting(EventStream eventStream)
        {
            var status = SynchronizeStatus.Success;

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
                            eventStream);
                        _logger.Error(errorMessage, ex);

                        if (ex is ConcurrentException)
                        {
                            status = SynchronizeStatus.SynchronizerConcurrentException;
                        }
                        else
                        {
                            status = SynchronizeStatus.Failed;
                        }
                        return status;
                    }
                }
            }

            return status;
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
                            eventStream), ex);
                    }
                }
            }
        }
        private void RetryCommand(EventCommittingContext context)
        {
            Func<bool> retryCommand = () =>
            {
                var eventStream = context.EventStream;

                var commandInfo = _processingCommandCache.Get(eventStream.CommandId);
                if (commandInfo != null)
                {
                    _retryCommandService.RetryCommand(commandInfo, context);
                }
                else
                {
                    _logger.ErrorFormat("The command need to retry cannot be found in command processing cache, commandId:{0}", eventStream.CommandId);
                }

                return true;
            };
            _actionExecutionService.TryAction("RetryCommand", retryCommand, 3, null);
        }
        private void CompleteCommandExecution(EventCommittingContext context)
        {
            _processingCommandCache.TryRemove(context.EventStream.CommandId);
            context.CommandExecuteContext.OnCommandExecuted(context.Command);
        }

        #endregion

        enum SynchronizeStatus
        {
            Success,
            SynchronizerConcurrentException,
            Failed
        }
    }
}
