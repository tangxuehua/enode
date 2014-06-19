using System;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of ICommitEventService.
    /// </summary>
    public class DefaultCommitEventService : ICommitEventService
    {
        #region Private Variables

        private readonly IExecutedCommandService _executedCommandService;
        private readonly IAggregateRootTypeCodeProvider _aggregateRootTypeCodeProvider;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IEventStreamConvertService _eventStreamConvertService;
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateStorage _aggregateStorage;
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
        /// <param name="executedCommandService"></param>
        /// <param name="aggregateRootTypeCodeProvider"></param>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="eventStreamConvertService"></param>
        /// <param name="eventSourcingService"></param>
        /// <param name="memoryCache"></param>
        /// <param name="aggregateStorage"></param>
        /// <param name="retryCommandService"></param>
        /// <param name="eventStore"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="eventSynchronizerProvider"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommitEventService(
            IExecutedCommandService executedCommandService,
            IAggregateRootTypeCodeProvider aggregateRootTypeCodeProvider,
            IAggregateRootFactory aggregateRootFactory,
            IEventStreamConvertService eventStreamConvertService,
            IEventSourcingService eventSourcingService,
            IMemoryCache memoryCache,
            IAggregateStorage aggregateStorage,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            IActionExecutionService actionExecutionService,
            IEventSynchronizerProvider eventSynchronizerProvider,
            ILoggerFactory loggerFactory)
        {
            _executedCommandService = executedCommandService;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _eventStreamConvertService = eventStreamConvertService;
            _eventSourcingService = eventSourcingService;
            _memoryCache = memoryCache;
            _aggregateStorage = aggregateStorage;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _eventPublisher = eventPublisher;
            _actionExecutionService = actionExecutionService;
            _eventSynchronizerProvider = eventSynchronizerProvider;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        /// <summary>Set the command executor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _retryCommandService.SetCommandExecutor(commandExecutor);
        }
        /// <summary>Commit the given aggregate's domain events to the eventstore and publish the domain events.
        /// </summary>
        public void CommitEvent(EventProcessingContext context)
        {
            _actionExecutionService.TryAction(
                "PersistEvents",
                () => PersistEvents(context),
                3,
                new ActionInfo("PersistEventsCallback", PersistEventCallback, context, null));
        }

        #region Private Methods

        private bool PersistEvents(EventProcessingContext context)
        {
            try
            {
                var record = _eventStreamConvertService.ConvertTo(context.EventStream);
                context.AppendResult = _eventStore.Append(record);
                _logger.DebugFormat("Persist events success. {0}", context.EventStream);
                return true;
            }
            catch (ConcurrentException ex)
            {
                context.Exception = ex;
                return true;
            }
            catch (DuplicateAggregateException ex)
            {
                context.Exception = ex;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("{0} raised when persisting events:{1}", ex.GetType().Name, context.EventStream), ex);
                return false;
            }
        }
        private bool PersistEventCallback(object obj)
        {
            var context = obj as EventProcessingContext;
            var eventStream = context.EventStream;

            if (context.AppendResult == EventAppendResult.Success)
            {
                RefreshMemoryCache(context);
                PublishEvents(context, eventStream);
            }
            else if (context.AppendResult == EventAppendResult.DuplicateCommit)
            {
                var existingEventStream = GetEventStream(eventStream.AggregateRootId, eventStream.CommitId);
                if (existingEventStream != null)
                {
                    PublishEvents(context, existingEventStream);
                }
                else
                {
                    var errorMessage = string.Format("Duplicate commit, but cannot find the existing eventstream from eventstore. commitId:{0}, aggregateRootId:{1}, aggregateRootTypeCode:{2}",
                        eventStream.CommitId,
                        eventStream.AggregateRootId,
                        eventStream.AggregateRootTypeCode);
                    _logger.Error(errorMessage);
                    NotifyCommandExecuted(context, CommandStatus.Failed, null, errorMessage);
                }
            }
            else if (context.Exception != null)
            {
                if (context.Exception is DuplicateAggregateException)
                {
                    NotifyCommandExecuted(context, CommandStatus.Failed, context.Exception.GetType().Name, context.Exception.Message);
                }
                else if (context.Exception is ConcurrentException)
                {
                    RefreshMemoryCacheFromEventStore(eventStream);
                    RetryCommand(context);
                }
            }

            return true;
        }
        private void RefreshMemoryCache(EventProcessingContext context)
        {
            try
            {
                _eventSourcingService.ReplayEvents(context.AggregateRoot, new EventStream[] { context.EventStream });
                _memoryCache.Set(context.AggregateRoot);
                _logger.DebugFormat("Memory cache refreshed, aggregateRootType:{0}, aggregateRootId:{1}, aggregateRootVersion:{2}", context.AggregateRoot.GetType().Name, context.AggregateRoot.UniqueId, context.AggregateRoot.Version);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache, current event stream:{0}", context.EventStream), ex);
            }
        }
        private void RefreshMemoryCacheFromEventStore(EventStream eventStream)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeCodeProvider.GetType(eventStream.AggregateRootTypeCode);
                if (aggregateRootType == null)
                {
                    _logger.ErrorFormat("Could not find aggregate root type by aggregate root type code [{0}].", eventStream.AggregateRootTypeCode);
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
                _logger.Error(string.Format("Exception raised when refreshing memory cache from eventstore, current event stream:{0}", eventStream), ex);
            }
        }
        private void PublishEvents(EventProcessingContext context, EventStream eventStream)
        {
            _actionExecutionService.TryAction(
                "PublishEvents",
                () => DoPublishEvents(context, eventStream),
                3,
                new ActionInfo("PublishEventsCallback", obj =>
                {
                    NotifyCommandExecuted(obj as EventProcessingContext, CommandStatus.Success, null, null);
                    return true;
                }, context, null));
        }
        private bool DoPublishEvents(EventProcessingContext context, EventStream eventStream)
        {
            try
            {
                _eventPublisher.PublishEvent(context.ProcessingCommand.CommandExecuteContext.Items, eventStream);
                _logger.DebugFormat("Publish events success. {0}", eventStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream), ex);
                return false;
            }
        }
        private void RetryCommand(EventProcessingContext context)
        {
            if (!_retryCommandService.RetryCommand(context.ProcessingCommand))
            {
                var command = context.ProcessingCommand.Command;
                var errorMessage = string.Format("{0} [id:{1}, aggregateId:{2}] retried count reached to its max retry count {3}.", command.GetType().Name, command.Id, context.EventStream.AggregateRootId, command.RetryCount);
                NotifyCommandExecuted(context, CommandStatus.Failed, null, errorMessage);
            }
        }
        private void NotifyCommandExecuted(EventProcessingContext context, CommandStatus commandStatus, string exceptionTypeName, string errorMessage)
        {
            _executedCommandService.ProcessExecutedCommand(
                context.ProcessingCommand.CommandExecuteContext,
                context.ProcessingCommand.Command,
                commandStatus,
                context.EventStream.ProcessId,
                context.EventStream.AggregateRootId,
                exceptionTypeName,
                errorMessage);
        }
        private EventStream GetEventStream(string aggregateRootId, string commitId)
        {
            var record = _eventStore.Find(aggregateRootId, commitId);
            if (record != null)
            {
                return _eventStreamConvertService.ConvertFrom(record);
            }
            return null;
        }

        #endregion
    }
}
