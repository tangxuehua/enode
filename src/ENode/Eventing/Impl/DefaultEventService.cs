using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class DefaultEventService : IEventService
    {
        #region Private Variables

        private readonly bool _enableGroupCommit;
        private readonly int _groupCommitInterval;
        private readonly int _groupCommitMaxCount;
        private readonly ConcurrentQueue<EventCommittingContext> _toCommittingEventQueue;
        private readonly BlockingCollection<IEnumerable<EventCommittingContext>> _successPersistedEventsQueue;
        private readonly BlockingCollection<IEnumerable<EventCommittingContext>> _failedPersistedEventsQueue;
        private readonly Worker _processSuccessPersistedEventsWorker;
        private readonly Worker _processFailedPersistedEventsWorker;
        private readonly IScheduleService _scheduleService;
        private readonly IExecutedCommandService _executedCommandService;
        private readonly ITypeCodeProvider<IAggregateRoot> _aggregateRootTypeCodeProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly IRetryCommandService _retryCommandService;
        private readonly IEventStore _eventStore;
        private readonly IMessagePublisher<DomainEventStream> _domainEventPublisher;
        private readonly IMessagePublisher<EventStream> _eventPublisher;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;
        private int _isBatchPersistingEvents;
        private long _toCommittingEventCount;

        #endregion

        #region Constructors

        public DefaultEventService(
            IScheduleService scheduleService,
            IExecutedCommandService executedCommandService,
            ITypeCodeProvider<IAggregateRoot> aggregateRootTypeCodeProvider,
            IMemoryCache memoryCache,
            IAggregateRootFactory aggregateRootFactory,
            IAggregateStorage aggregateStorage,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IMessagePublisher<DomainEventStream> domainEventPublisher,
            IMessagePublisher<EventStream> eventPublisher,
            IActionExecutionService actionExecutionService,
            IEventPublishInfoStore eventPublishInfoStore,
            ILoggerFactory loggerFactory)
        {
            var setting = ENodeConfiguration.Instance.Setting;
            _enableGroupCommit = setting.EnableGroupCommitEvent;
            _groupCommitInterval = setting.GroupCommitEventInterval;
            _groupCommitMaxCount = setting.GroupCommitEventMaxCount;
            Ensure.Positive(_groupCommitInterval, "GroupCommitEventInterval");
            Ensure.Positive(_groupCommitMaxCount, "GroupCommitEventMaxCount");

            _toCommittingEventQueue = new ConcurrentQueue<EventCommittingContext>();
            _successPersistedEventsQueue = new BlockingCollection<IEnumerable<EventCommittingContext>>();
            _failedPersistedEventsQueue = new BlockingCollection<IEnumerable<EventCommittingContext>>();

            _scheduleService = scheduleService;
            _executedCommandService = executedCommandService;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
            _memoryCache = memoryCache;
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateStorage = aggregateStorage;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _domainEventPublisher = domainEventPublisher;
            _eventPublisher = eventPublisher;
            _eventPublishInfoStore = eventPublishInfoStore;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().FullName);
            _processSuccessPersistedEventsWorker = new Worker("ProcessSuccessPersistedEvents", ProcessSuccessPersistedEvents);
            _processFailedPersistedEventsWorker = new Worker("ProcessFailedPersistedEvents", ProcessFailedPersistedEvents);
        }

        #endregion

        public void Start()
        {
            if (_enableGroupCommit)
            {
                _scheduleService.ScheduleTask("TryBatchPersistEvents", TryBatchPersistEvents, _groupCommitInterval, _groupCommitInterval);
                _processSuccessPersistedEventsWorker.Start();
                _processFailedPersistedEventsWorker.Start();
            }
        }
        public void CommitEvent(EventCommittingContext context)
        {
            if (_enableGroupCommit)
            {
                _toCommittingEventQueue.Enqueue(context);
                Interlocked.Increment(ref _toCommittingEventCount);
            }
            else
            {
                _actionExecutionService.TryAction(
                    "PersistEvent",
                    () => PersistEvent(context),
                    3,
                    new ActionInfo("PersistEventCallback", PersistEventCallback, context, null));
            }
        }
        public void PublishDomainEvent(ProcessingCommand processingCommand, DomainEventStream eventStream)
        {
            _actionExecutionService.TryAction(
                "PublishDomainEvent",
                () =>
                {
                    try
                    {
                        eventStream.Items["DomainEventHandledMessageTopic"] = processingCommand.Command.Items["DomainEventHandledMessageTopic"];
                        _domainEventPublisher.Publish(eventStream);
                        _logger.DebugFormat("Publish domain event success, commandId:{0}, aggregateRootId:{1}, aggregateRootTypeCode:{2}, events:{3}",
                            processingCommand.Command.Id,
                            eventStream.AggregateRootId,
                            eventStream.AggregateRootTypeCode,
                            string.Join("|", eventStream.Events.Select(x => x.GetType().Name)));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Exception raised when publishing domain event, commandId:{0}, aggregateRootId:{1}, aggregateRootTypeCode:{2}, events:{3}",
                            processingCommand.Command.Id,
                            eventStream.AggregateRootId,
                            eventStream.AggregateRootTypeCode,
                            string.Join("|", eventStream.Events.Select(x => x.GetType().Name))
                        ), ex);
                        return false;
                    }
                },
                3,
                new ActionInfo("PublishDomainEventCallback", obj =>
                {
                    NotifyCommandExecuted(processingCommand, eventStream.AggregateRootId, CommandStatus.Success, null, null);
                    return true;
                }, null, null));
        }
        public void PublishEvent(ProcessingCommand processingCommand, EventStream eventStream)
        {
            _actionExecutionService.TryAction(
                "PublishEvent",
                () =>
                {
                    try
                    {
                        _eventPublisher.Publish(eventStream);
                        _logger.DebugFormat("Publish event success, commandId:{0}, events:{1}",
                            eventStream.CommandId,
                            string.Join("|", eventStream.Events.Select(x => x.GetType().Name)));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Exception raised when publishing event, commandId:{0}, events:{1}",
                            eventStream.CommandId,
                            string.Join("|", eventStream.Events.Select(x => x.GetType().Name))
                        ), ex);
                        return false;
                    }
                },
                3,
                new ActionInfo("PublishEventCallback", obj =>
                {
                    NotifyCommandExecuted(processingCommand, null, CommandStatus.Success, null, null);
                    return true;
                }, null, null));
        }

        #region Private Methods

        private void TryBatchPersistEvents()
        {
            if (Interlocked.CompareExchange(ref _isBatchPersistingEvents, 1, 0) == 0)
            {
                try
                {
                    var hasNextBatch = BatchPersistEvents();
                    while (hasNextBatch)
                    {
                        hasNextBatch = BatchPersistEvents();
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _isBatchPersistingEvents, 0);
                }
            }
        }
        private bool BatchPersistEvents()
        {
            var eventCommittingContextList = new List<EventCommittingContext>();
            var eventContextCount = 0;
            EventCommittingContext context;

            while (eventContextCount < _groupCommitMaxCount && _toCommittingEventQueue.TryDequeue(out context))
            {
                Interlocked.Decrement(ref _toCommittingEventCount);
                eventCommittingContextList.Add(context);
                eventContextCount++;
            }

            if (eventContextCount == 0)
            {
                return false;
            }

            try
            {
                var appendResult = _eventStore.BatchAppend(eventCommittingContextList.Select(x => x.EventStream));
                if (appendResult == EventAppendResult.Success)
                {
                    _logger.DebugFormat("Batch persist event stream success, persisted event stream count:{0}", eventContextCount);
                    _successPersistedEventsQueue.Add(eventCommittingContextList);
                }
            }
            catch (Exception ex)
            {
                _logger.DebugFormat(string.Format("Batch persist event stream failed, current event stream count:{0}", eventContextCount), ex);
                _failedPersistedEventsQueue.Add(eventCommittingContextList);
            }

            return _toCommittingEventCount >= _groupCommitMaxCount;
        }
        private void ProcessSuccessPersistedEvents()
        {
            var eventCommittingContextList = _successPersistedEventsQueue.Take();
            foreach (var context in eventCommittingContextList)
            {
                RefreshAggregateMemoryCache(context);
                _actionExecutionService.TryAction("PublishDomainEvent", () =>
                {
                    PublishDomainEvent(context.ProcessingCommand, context.EventStream);
                    return true;
                }, 3, null);
            }
        }
        private void ProcessFailedPersistedEvents()
        {
            var eventCommittingContextList = _failedPersistedEventsQueue.Take();
            foreach (var context in eventCommittingContextList)
            {
                _actionExecutionService.TryAction(
                    "PersistEvent",
                    () => PersistEvent(context),
                    3,
                    new ActionInfo("PersistEventCallback", PersistEventCallback, context, null));
            }
        }
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
                RefreshAggregateMemoryCache(context);
                PublishDomainEvent(context.ProcessingCommand, eventStream);
            }
            //如果事件持久化遇到重复的情况
            else if (context.EventAppendResult == EventAppendResult.DuplicateEvent)
            {
                //如果是当前事件的版本号为1，则认为是在创建重复的聚合根
                if (eventStream.Version == 1)
                {
                    //取出该聚合根版本号为1的事件
                    var firstEventStream = _eventStore.Find(eventStream.AggregateRootId, 1);
                    if (firstEventStream != null)
                    {
                        //判断是否是同一个command，如果是，则再重新做一遍更新内存缓存以及发布事件这两个操作；
                        //之所以要这样做，是因为虽然该command产生的事件已经持久化成功，但并不表示已经内存也更新了或者事件已经发布出去了；
                        //有可能事件持久化成功了，但那时正好机器断电了，则更新内存和发布事件都没有做；
                        if (context.ProcessingCommand.Command.Id == firstEventStream.CommandId)
                        {
                            RefreshAggregateMemoryCache(firstEventStream);
                            PublishDomainEvent(context.ProcessingCommand, firstEventStream);
                        }
                        else
                        {
                            //如果不是同一个command，则认为是两个不同的command重复创建ID相同的聚合根，我们需要记录错误日志，然后通知当前command的处理完成；
                            var errorMessage = string.Format("Duplicate aggregate creation. current commandId:{0}, existing commandId:{1}, aggregateRootId:{2}, aggregateRootTypeCode:{3}",
                                context.ProcessingCommand.Command.Id,
                                eventStream.CommandId,
                                eventStream.AggregateRootId,
                                eventStream.AggregateRootTypeCode);
                            _logger.Error(errorMessage);
                            NotifyCommandExecuted(context.ProcessingCommand, eventStream.AggregateRootId, CommandStatus.Failed, null, errorMessage);
                        }
                    }
                    else
                    {
                        var errorMessage = string.Format("Duplicate aggregate creation, but cannot find the existing eventstream from eventstore. commandId:{0}, aggregateRootId:{1}, aggregateRootTypeCode:{2}",
                            eventStream.CommandId,
                            eventStream.AggregateRootId,
                            eventStream.AggregateRootTypeCode);
                        _logger.Error(errorMessage);
                        NotifyCommandExecuted(context.ProcessingCommand, eventStream.AggregateRootId, CommandStatus.Failed, null, errorMessage);
                    }
                }
                //如果事件的版本大于1，则认为是更新聚合根时遇到并发冲突了；
                //那么我们需要先将聚合根的最新状态更新到内存，然后重试command；
                else
                {
                    UpdateAggregateToLatestVersion(eventStream.AggregateRootTypeCode, eventStream.AggregateRootId);
                    RetryConcurrentCommand(context);
                }
            }

            return true;
        }
        private void RefreshAggregateMemoryCache(DomainEventStream aggregateFirstEventStream)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeCodeProvider.GetType(aggregateFirstEventStream.AggregateRootTypeCode);
                var aggregateRoot = _memoryCache.Get(aggregateFirstEventStream.AggregateRootId, aggregateRootType);
                if (aggregateRoot == null)
                {
                    aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                    aggregateRoot.ReplayEvents(new DomainEventStream[] { aggregateFirstEventStream });
                    _memoryCache.Set(aggregateRoot);
                    _logger.DebugFormat("Aggregate added into memory, commandId:{0}, aggregateRootType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}", aggregateFirstEventStream.CommandId, aggregateRootType.Name, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when adding aggregate to memory, the first event stream info of the aggregate:{0}", aggregateFirstEventStream), ex);
            }
        }
        private void RefreshAggregateMemoryCache(EventCommittingContext context)
        {
            try
            {
                _memoryCache.Set(context.AggregateRoot);
                _logger.DebugFormat("Refreshed aggregate memory cache, commandId:{0}, aggregateRootType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}", context.EventStream.CommandId, context.AggregateRoot.GetType().Name, context.AggregateRoot.UniqueId, context.AggregateRoot.Version);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing memory cache, current event stream:{0}", context.EventStream), ex);
            }
        }
        private void UpdateAggregateToLatestVersion(int aggregateRootTypeCode, string aggregateRootId)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeCodeProvider.GetType(aggregateRootTypeCode);
                if (aggregateRootType == null)
                {
                    _logger.ErrorFormat("Could not find aggregate root type by aggregate root type code [{0}].", aggregateRootTypeCode);
                    return;
                }
                var aggregateRoot = _aggregateStorage.Get(aggregateRootType, aggregateRootId);
                if (aggregateRoot != null)
                {
                    _memoryCache.Set(aggregateRoot);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when update aggregate to latest version, aggregateRootTypeCode:{0}, aggregateRootId:{1}", aggregateRootTypeCode, aggregateRootId), ex);
            }
        }
        private void RetryConcurrentCommand(EventCommittingContext context)
        {
            if (!_retryCommandService.RetryConcurrentCommand(context.ProcessingCommand))
            {
                var command = context.ProcessingCommand.Command;
                var errorMessage = string.Format("{0} [id:{1}, aggregateId:{2}] retried count reached to its max retry count {3}.", command.GetType().Name, command.Id, context.EventStream.AggregateRootId, command.RetryCount);
                NotifyCommandExecuted(context.ProcessingCommand, context.EventStream.AggregateRootId, CommandStatus.Failed, null, errorMessage);
            }
        }
        private void NotifyCommandExecuted(ProcessingCommand processingCommand, string aggregateRootId, CommandStatus commandStatus, string exceptionTypeName, string errorMessage)
        {
            var aggregateCommand = processingCommand.Command as IAggregateCommand;
            if (aggregateCommand != null)
            {
                _executedCommandService.ProcessExecutedCommand(
                    processingCommand.CommandExecuteContext,
                    aggregateCommand,
                    commandStatus,
                    aggregateRootId,
                    exceptionTypeName,
                    errorMessage);
            }
            else
            {
                _executedCommandService.ProcessExecutedCommand(
                    processingCommand.CommandExecuteContext,
                    processingCommand.Command,
                    commandStatus,
                    exceptionTypeName,
                    errorMessage);
            }
        }

        #endregion
    }
}
