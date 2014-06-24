using System;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventService.
    /// </summary>
    public class DefaultEventService : IEventService
    {
        #region Private Variables

        private readonly IExecutedCommandService _executedCommandService;
        private readonly IAggregateRootTypeCodeProvider _aggregateRootTypeCodeProvider;
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly IRetryCommandService _retryCommandService;
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="executedCommandService"></param>
        /// <param name="aggregateRootTypeCodeProvider"></param>
        /// <param name="eventSourcingService"></param>
        /// <param name="memoryCache"></param>
        /// <param name="aggregateStorage"></param>
        /// <param name="retryCommandService"></param>
        /// <param name="eventStore"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultEventService(
            IExecutedCommandService executedCommandService,
            IAggregateRootTypeCodeProvider aggregateRootTypeCodeProvider,
            IEventSourcingService eventSourcingService,
            IMemoryCache memoryCache,
            IAggregateStorage aggregateStorage,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _executedCommandService = executedCommandService;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
            _eventSourcingService = eventSourcingService;
            _memoryCache = memoryCache;
            _aggregateStorage = aggregateStorage;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _eventPublisher = eventPublisher;
            _actionExecutionService = actionExecutionService;
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
        /// <param name="context"></param>
        public void CommitEvent(EventCommittingContext context)
        {
            _actionExecutionService.TryAction(
                "PersistEvent",
                () => PersistEvent(context),
                3,
                new ActionInfo("PersistEventCallback", PersistEventCallback, context, null));
        }
        /// <summary>Publish the given aggregate's domain events.
        /// </summary>
        /// <param name="processingCommand"></param>
        /// <param name="eventStream"></param>
        public void PublishEvent(ProcessingCommand processingCommand, EventStream eventStream)
        {
            _actionExecutionService.TryAction(
                "PublishEvent",
                () =>
                {
                    try
                    {
                        var processId = processingCommand.Command is IProcessCommand ? ((IProcessCommand)processingCommand.Command).ProcessId : null;
                        processingCommand.CommandExecuteContext.Items["CurrentCommandId"] = processingCommand.Command.Id;
                        processingCommand.CommandExecuteContext.Items["CurrentProcessId"] = processId;
                        _eventPublisher.PublishEvent(processingCommand.CommandExecuteContext.Items, eventStream);
                        _logger.DebugFormat("Publish event success. {0}", eventStream);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Exception raised when publishing event:{0}", eventStream), ex);
                        return false;
                    }
                },
                3,
                new ActionInfo("PublishEventCallback", obj =>
                {
                    NotifyCommandExecuted(processingCommand, eventStream, CommandStatus.Success, null, null);
                    return true;
                }, null, null));
        }

        #region Private Methods

        private bool PersistEvent(EventCommittingContext context)
        {
            try
            {
                context.EventAppendResult = _eventStore.Append(context.EventStream);
                if (context.EventAppendResult == EventAppendResult.Success)
                {
                    _logger.DebugFormat("Persist event success. {0}", context.EventStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("{0} raised when persisting event:{1}", ex.GetType().Name, context.EventStream), ex);
                return false;
            }
        }
        private bool PersistEventCallback(object obj)
        {
            var context = obj as EventCommittingContext;
            var eventStream = context.EventStream;

            if (context.EventAppendResult == EventAppendResult.Success)
            {
                RefreshMemoryCache(context);
                PublishEvent(context.ProcessingCommand, eventStream);
            }
            else if (context.EventAppendResult == EventAppendResult.DuplicateEvent)
            {
                if (eventStream.Version == 1)
                {
                    var existingEventStream = _eventStore.Find(eventStream.AggregateRootId, 1);
                    if (existingEventStream != null)
                    {
                        RefreshMemoryCacheFromEventStore(existingEventStream);
                        PublishEvent(context.ProcessingCommand, existingEventStream);
                    }
                    else
                    {
                        var errorMessage = string.Format("Duplicate aggregate creation, but cannot find the existing eventstream from eventstore. commitId:{0}, aggregateRootId:{1}, aggregateRootTypeCode:{2}",
                            eventStream.CommitId,
                            eventStream.AggregateRootId,
                            eventStream.AggregateRootTypeCode);
                        _logger.Error(errorMessage);
                        NotifyCommandExecuted(context.ProcessingCommand, eventStream, CommandStatus.Failed, null, errorMessage);
                    }
                }
                else
                {
                    RefreshMemoryCacheFromEventStore(eventStream);
                    RetryCommand(context);
                }
            }

            return true;
        }
        private void RefreshMemoryCache(EventCommittingContext context)
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
                _logger.Error(string.Format("Exception raised when refreshing memory cache from eventstore, current event:{0}", eventStream), ex);
            }
        }
        private void RetryCommand(EventCommittingContext context)
        {
            if (!_retryCommandService.RetryCommand(context.ProcessingCommand))
            {
                var command = context.ProcessingCommand.Command;
                var errorMessage = string.Format("{0} [id:{1}, aggregateId:{2}] retried count reached to its max retry count {3}.", command.GetType().Name, command.Id, context.EventStream.AggregateRootId, command.RetryCount);
                NotifyCommandExecuted(context.ProcessingCommand, context.EventStream, CommandStatus.Failed, null, errorMessage);
            }
        }
        private void NotifyCommandExecuted(ProcessingCommand processingCommand, EventStream eventStream, CommandStatus commandStatus, string exceptionTypeName, string errorMessage)
        {
            _executedCommandService.ProcessExecutedCommand(
                processingCommand.CommandExecuteContext,
                processingCommand.Command,
                commandStatus,
                eventStream.ProcessId,
                eventStream.AggregateRootId,
                exceptionTypeName,
                errorMessage);
        }

        #endregion
    }
}
