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
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly IRetryCommandService _retryCommandService;
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
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
        /// <param name="aggregateRootFactory"></param>
        /// <param name="aggregateStorage"></param>
        /// <param name="retryCommandService"></param>
        /// <param name="eventStore"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="eventPublishInfoStore"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultEventService(
            IExecutedCommandService executedCommandService,
            IAggregateRootTypeCodeProvider aggregateRootTypeCodeProvider,
            IEventSourcingService eventSourcingService,
            IMemoryCache memoryCache,
            IAggregateRootFactory aggregateRootFactory,
            IAggregateStorage aggregateStorage,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            IActionExecutionService actionExecutionService,
            IEventPublishInfoStore eventPublishInfoStore,
            ILoggerFactory loggerFactory)
        {
            _executedCommandService = executedCommandService;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
            _eventSourcingService = eventSourcingService;
            _memoryCache = memoryCache;
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateStorage = aggregateStorage;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _eventPublisher = eventPublisher;
            _eventPublishInfoStore = eventPublishInfoStore;
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
                        _logger.DebugFormat("Publish event success, commandId:{0}", eventStream.CommandId);
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
                    _logger.DebugFormat("Persist event success, {0}", context.EventStream);
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

            //如果事件持久化成功
            if (context.EventAppendResult == EventAppendResult.Success)
            {
                //刷新内存缓存并发布事件
                RefreshMemoryCache(context);
                PublishEvent(context.ProcessingCommand, eventStream);
            }
            //如果事件持久化遇到重复的情况
            else if (context.EventAppendResult == EventAppendResult.DuplicateEvent)
            {
                //如果是当前事件的版本号为1，则认为是在创建重复的聚合根
                if (eventStream.Version == 1)
                {
                    //取出该聚合根版本号为1的事件，然后再重新做一遍更新内存缓存以及发布事件这两个操作；
                    //之所以要这样做，是因为虽然该事件已经持久化成功，但并不表示已经内存也更新了或者事件已经发布出去了；
                    //有可能事件持久化成功了，但那时正好机器断电了，则更新内存和发布事件都没有做；
                    var firstEventStream = _eventStore.Find(eventStream.AggregateRootId, 1);
                    if (firstEventStream != null)
                    {
                        RefreshMemoryCache(firstEventStream);
                        PublishEvent(context.ProcessingCommand, firstEventStream);
                    }
                    else
                    {
                        var errorMessage = string.Format("Duplicate aggregate creation, but cannot find the existing eventstream from eventstore. commandId:{0}, aggregateRootId:{1}, aggregateRootTypeCode:{2}",
                            eventStream.CommandId,
                            eventStream.AggregateRootId,
                            eventStream.AggregateRootTypeCode);
                        _logger.Error(errorMessage);
                        NotifyCommandExecuted(context.ProcessingCommand, eventStream, CommandStatus.Failed, null, errorMessage);
                    }
                }
                //如果事件的版本大于1，则认为是更新聚合根时遇到并发冲突了；
                //那么我么需要先将聚合根的最新状态更新到内存，然后重试command；
                else
                {
                    RefreshMemoryCacheFromEventStore(eventStream);
                    RetryCommand(context);
                }
            }

            return true;
        }
        private void RefreshMemoryCache(EventStream firstEventStream)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeCodeProvider.GetType(firstEventStream.AggregateRootTypeCode);
                var aggregateRoot = _memoryCache.Get(firstEventStream.AggregateRootId, aggregateRootType);
                if (aggregateRoot == null)
                {
                    aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                    _eventSourcingService.ReplayEvents(aggregateRoot, new EventStream[] { firstEventStream });
                    _memoryCache.Set(aggregateRoot);
                    _logger.DebugFormat("Memory cache refreshed, commandId:{0}, aggregateRootType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}", firstEventStream.CommandId, aggregateRootType.Name, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache, current event stream:{0}", firstEventStream), ex);
            }
        }
        private void RefreshMemoryCache(EventCommittingContext context)
        {
            try
            {
                _eventSourcingService.ReplayEvents(context.AggregateRoot, new EventStream[] { context.EventStream });
                _memoryCache.Set(context.AggregateRoot);
                _logger.DebugFormat("Memory cache refreshed, commandId:{0}, aggregateRootType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}", context.EventStream.CommandId, context.AggregateRoot.GetType().Name, context.AggregateRoot.UniqueId, context.AggregateRoot.Version);
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
