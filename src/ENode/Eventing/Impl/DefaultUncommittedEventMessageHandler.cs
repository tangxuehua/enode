using System;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IUncommittedEventMessageHandler.
    /// </summary>
    public class DefaultUncommittedEventMessageHandler : MessageHandler<EventStream>, IUncommittedEventMessageHandler
    {
        #region Private Variables

        private readonly ICommandCompletionEventManager _commandCompletionEventManager;
        private readonly ICommandTaskManager _commandTaskManager;
        private readonly IWaitingCommandService _waitingCommandService;
        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IMemoryCache _memoryCache;
        private readonly IRepository _repository;
        private readonly IRetryCommandService _retryCommandService;
        private readonly IEventStore _eventStore;
        private readonly ICommittedEventSender _committedEventSender;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly IEventSynchronizerProvider _eventSynchronizerProvider;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandCompletionEventManager"></param>
        /// <param name="commandTaskManager"></param>
        /// <param name="waitingCommandService"></param>
        /// <param name="processingCommandCache"></param>
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
        public DefaultUncommittedEventMessageHandler(
            ICommandCompletionEventManager commandCompletionEventManager,
            ICommandTaskManager commandTaskManager,
            IWaitingCommandService waitingCommandService,
            IProcessingCommandCache processingCommandCache,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IAggregateRootFactory aggregateRootFactory,
            IEventSourcingService eventSourcingService,
            IMemoryCache memoryCache,
            IRepository repository,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            ICommittedEventSender committedEventSender,
            IActionExecutionService actionExecutionService,
            IEventSynchronizerProvider eventSynchronizerProvider,
            ILoggerFactory loggerFactory)
        {
            _commandCompletionEventManager = commandCompletionEventManager;
            _commandTaskManager = commandTaskManager;
            _waitingCommandService = waitingCommandService;
            _processingCommandCache = processingCommandCache;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _eventSourcingService = eventSourcingService;
            _memoryCache = memoryCache;
            _repository = repository;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _committedEventSender = committedEventSender;
            _actionExecutionService = actionExecutionService;
            _eventSynchronizerProvider = eventSynchronizerProvider;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        /// <summary>Handle the given event stream message.
        /// </summary>
        /// <param name="message"></param>
        public override void Handle(Message<EventStream> message)
        {
            //TODO
            //var context = new EventStreamContext { EventStream = eventStream, Queue = queue };

            //Func<bool> commitEvents = () =>
            //{
            //    try
            //    {
            //        return CommitEvents(context);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.Error(string.Format("Exception raised when committing events:{0}.", context.EventStream.GetStreamInformation()), ex);
            //        return false;
            //    }
            //};

            //_actionExecutionService.TryAction("CommitEvents", commitEvents, 3, null);
        }

        #region Private Methods

        private bool CommitEvents(EventProcessingContext context)
        {
            var synchronizeResult = SyncBeforeEventPersisting(context.EventStream);

            switch (synchronizeResult.Status)
            {
                case SynchronizeStatus.SynchronizerConcurrentException:
                    return false;
                case SynchronizeStatus.Failed:
                    CompleteCommandTask(context.EventStream, synchronizeResult.Exception);
                    CleanEvents(context);
                    return true;
                default:
                {
                    Func<object, bool> persistEventsCallback = (obj) =>
                    {
                        var eventStream = context.EventStream;
                        //TODO
                        //if (context.ConcurrentException != null)
                        //{
                        //    RefreshMemoryCache(eventStream);
                        //    RetryCommand(context, context.ConcurrentException, new ActionInfo("RetryCommandCallback", data => { context.Queue.Delete(eventStream); return true; }, null, null));
                        //}
                        //else
                        //{
                        //    RefreshMemoryCache(eventStream);
                        //    CompleteCommandTask(eventStream, null);
                        //    SendWaitingCommand(eventStream);
                        //    SyncAfterEventPersisted(eventStream);
                        //    PublishEvents(eventStream, new ActionInfo("PublishEventsCallback", data => { CleanEvents(context); return true; }, null, null));
                        //}
                        return true;
                    };

                    PersistEvents(context, new ActionInfo("PersistEventsCallback", persistEventsCallback, null, null));

                    return true;
                }
            }
        }
        private void PersistEvents(EventProcessingContext context, ActionInfo successCallback)
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
                    var errorMessage = string.Format("{0} raised when persisting events:{1}", ex.GetType().Name, context.EventStream.GetStreamInformation());
                    _logger.Error(errorMessage, ex);

                    if (ex is ConcurrentException)
                    {
                        //context.SetConcurrentException(ex as ConcurrentException);
                        return true;
                    }

                    return false;
                }
            };

            _actionExecutionService.TryAction("PersistEvents", persistEvents, 3, successCallback);
        }
        private void CompleteCommandTask(EventStream eventStream, Exception exception)
        {
            foreach (var evnt in eventStream.Events)
            {
                if (_commandCompletionEventManager.IsCompletionEvent(evnt))
                {
                    if (exception == null)
                    {
                        _commandTaskManager.CompleteCommandTask(eventStream.CommandId);
                    }
                    else
                    {
                        _commandTaskManager.CompleteCommandTask(eventStream.CommandId, exception);
                    }
                    break;
                }
            }
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
                    _committedEventSender.Send(eventStream);
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
                        result.Exception = ex;
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
        private void RetryCommand(EventProcessingContext context, ConcurrentException concurrentException, ActionInfo successCallback)
        {
            Func<bool> retryCommand = () =>
            {
                var eventStream = context.EventStream;

                var commandInfo = _processingCommandCache.Get(eventStream.CommandId);
                if (commandInfo != null)
                {
                    _retryCommandService.RetryCommand(commandInfo, eventStream, concurrentException);
                }
                else
                {
                    _logger.ErrorFormat("The command need to retry cannot be found from command processing cache, commandId:{0}", eventStream.CommandId);
                }

                return true;
            };
            _actionExecutionService.TryAction("RetryCommand", retryCommand, 3, successCallback);
        }
        private void CleanEvents(EventProcessingContext context)
        {
            _processingCommandCache.TryRemove(context.EventStream.CommandId);
            //TODO
            //context.Queue.Delete(context.EventStream);
        }

        #endregion

        class SynchronizeResult
        {
            public SynchronizeStatus Status { get; set; }
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
