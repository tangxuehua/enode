using System;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>The default implementation of ICommitService.
    /// </summary>
    public class DefaultCommitEventService : ICommitEventService
    {
        #region Private Variables

        private readonly IWaitingCommandService _waitingCommandService;
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
        /// <param name="waitingCommandService"></param>
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
            IWaitingCommandService waitingCommandService,
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
            _waitingCommandService = waitingCommandService;
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

        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _retryCommandService.SetCommandExecutor(commandExecutor);
            _waitingCommandService.SetCommandExecutor(commandExecutor);
        }
        /// <summary>Commit the dirty aggregate's domain events to the eventstore and publish the domain events.
        /// </summary>
        public void CommitEvent(EventProcessingContext context)
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

        private bool CommitEvents(EventProcessingContext context)
        {
            var synchronizerResult = SyncBeforeEventPersisting(context.EventStream);

            switch (synchronizerResult.Status)
            {
                case SynchronizeStatus.SynchronizerConcurrentException:
                    return false;
                case SynchronizeStatus.Failed:
                    context.ProcessingCommand.CommandExecuteContext.OnCommandExecuted(new CommandResult(context.ProcessingCommand.Command, synchronizerResult.ErrorMessage));
                    return true;
                default:
                {
                    var persistEventsCallback = new Func<object, bool>(obj =>
                    {
                        var currentContext = obj as EventProcessingContext;
                        var eventStream = currentContext.EventStream;

                        if (currentContext.CommitStatus == EventCommitStatus.Success)
                        {
                            RefreshMemoryCache(currentContext);
                            SendWaitingCommand(eventStream);
                            SyncAfterEventPersisted(eventStream);
                            PublishEvents(currentContext);
                        }
                        else if (currentContext.CommitStatus == EventCommitStatus.DuplicateCommit)
                        {
                            SendWaitingCommand(eventStream);
                            SyncAfterEventPersisted(eventStream);
                            PublishEvents(currentContext);
                        }
                        else if (currentContext.Exception != null)
                        {
                            if (currentContext.Exception is DuplicateAggregateException)
                            {
                                currentContext.ProcessingCommand.CommandExecuteContext.OnCommandExecuted(new CommandResult(currentContext.ProcessingCommand.Command, currentContext.Exception));
                            }
                            else if (currentContext.Exception is ConcurrentException)
                            {
                                RefreshMemoryCacheFromEventStore(eventStream);
                                RetryCommand(currentContext);
                            }
                        }
                        return true;
                    });

                    PersistEvents(context, new ActionInfo("PersistEventsCallback", persistEventsCallback, context, null));
                    return true;
                }
            }
        }
        private void PersistEvents(EventProcessingContext context, ActionInfo successCallback)
        {
            var persistEvents = new Func<bool>(() =>
            {
                try
                {
                    context.CommitStatus = _eventStore.Commit(context.EventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex is ConcurrentException || ex is DuplicateAggregateException)
                    {
                        context.Exception = ex as ENodeException;
                        return true;
                    }
                    _logger.Error(string.Format("{0} raised when persisting events:{1}", ex.GetType().Name, context.EventStream), ex);
                    return false;
                }
            });

            _actionExecutionService.TryAction("PersistEvents", persistEvents, 3, successCallback);
        }
        private void SendWaitingCommand(EventStream eventStream)
        {
            _waitingCommandService.SendWaitingCommand(eventStream.AggregateRootId);
        }
        private void RefreshMemoryCache(EventProcessingContext context)
        {
            try
            {
                //TODO, check whether the current aggregate is the latest version.
                _eventSourcingService.ReplayEvents(context.AggregateRoot, new EventStream[] { context.EventStream });
                _memoryCache.Set(context.AggregateRoot);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache by event stream:{0}", context.EventStream), ex);
            }
        }
        private void RefreshMemoryCacheFromEventStore(EventStream eventStream)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName);
                if (aggregateRootType == null)
                {
                    _logger.ErrorFormat("Could not find aggregate root type by aggregate root name [{0}].", eventStream.AggregateRootName);
                    return;
                }
                var aggregateRoot = _aggregateStorage.Get(aggregateRootType, eventStream.AggregateRootId);
                if (aggregateRoot != null)
                {
                    _memoryCache.Set(aggregateRoot);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache from eventstore, current eventStream:{0}", eventStream), ex);
            }
        }
        private void PublishEvents(EventProcessingContext context)
        {
            _publishEventService.PublishEvent(context);
        }
        private SynchronizeResult SyncBeforeEventPersisting(EventStream eventStream)
        {
            var result = new SynchronizeResult { Status = SynchronizeStatus.Success };

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
                        if (ex is ConcurrentException)
                        {
                            result.Status = SynchronizeStatus.SynchronizerConcurrentException;
                            return result;
                        }

                        var errorMessage = string.Format("Exception raised when calling synchronizer's OnBeforePersisting method. synchronizer:{0}, events:{1}",
                            synchronizer.GetInnerSynchronizer().GetType().Name,
                            eventStream);
                        _logger.Error(errorMessage, ex);
                        result.Status = SynchronizeStatus.Failed;
                        result.ErrorMessage = errorMessage;
                        result.Exception = ex;
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
                            eventStream), ex);
                    }
                }
            }
        }
        private void RetryCommand(EventProcessingContext context)
        {
            _retryCommandService.RetryCommand(context.ProcessingCommand);
        }

        #endregion

        class SynchronizeResult
        {
            public SynchronizeStatus Status { get; set; }
            public string ErrorMessage { get; set; }
            public Exception Exception { get; set; }
        }
        enum SynchronizeStatus
        {
            Success,
            SynchronizerConcurrentException,
            Failed
        }
    }
}
