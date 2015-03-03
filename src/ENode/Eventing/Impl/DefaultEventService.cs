using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ECommon.Logging;
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

        private ICommandExecutor _commandExecutor;
        private readonly IScheduleService _scheduleService;
        private readonly ITypeCodeProvider _aggregateRootTypeCodeProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly IEventStore _eventStore;
        private readonly IPublisher<DomainEventStream> _domainEventPublisher;
        private readonly IPublisher<EventStream> _eventPublisher;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly bool _enableGroupCommit;
        private readonly int _groupCommitInterval;
        private readonly int _groupCommitMaxSize;
        private readonly ConcurrentQueue<EventCommittingContext> _toCommitContextQueue;
        private readonly BlockingCollection<IEnumerable<EventCommittingContext>> _successPersistedContextQueue;
        private readonly BlockingCollection<IEnumerable<EventCommittingContext>> _failedPersistedContextQueue;
        private readonly Worker _processSuccessPersistedEventsWorker;
        private readonly Worker _processFailedPersistedEventsWorker;
        private int _isBatchPersistingEvents;

        #endregion

        #region Constructors

        public DefaultEventService(
            IScheduleService scheduleService,
            ITypeCodeProvider aggregateRootTypeCodeProvider,
            IMemoryCache memoryCache,
            IAggregateRootFactory aggregateRootFactory,
            IAggregateStorage aggregateStorage,
            IEventStore eventStore,
            IPublisher<DomainEventStream> domainEventPublisher,
            IPublisher<EventStream> eventPublisher,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            var setting = ENodeConfiguration.Instance.Setting;
            _enableGroupCommit = setting.EnableGroupCommitEvent;
            _groupCommitInterval = setting.GroupCommitEventInterval;
            _groupCommitMaxSize = setting.GroupCommitMaxSize;
            _ioHelper = ioHelper;
            Ensure.Positive(_groupCommitInterval, "_groupCommitInterval");
            Ensure.Positive(_groupCommitMaxSize, "_groupCommitMaxSize");

            _toCommitContextQueue = new ConcurrentQueue<EventCommittingContext>();
            _successPersistedContextQueue = new BlockingCollection<IEnumerable<EventCommittingContext>>();
            _failedPersistedContextQueue = new BlockingCollection<IEnumerable<EventCommittingContext>>();

            _scheduleService = scheduleService;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
            _memoryCache = memoryCache;
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateStorage = aggregateStorage;
            _eventStore = eventStore;
            _domainEventPublisher = domainEventPublisher;
            _eventPublisher = eventPublisher;
            _logger = loggerFactory.Create(GetType().FullName);
            _processSuccessPersistedEventsWorker = new Worker("ProcessSuccessPersistedEvents", ProcessSuccessPersistedEvents);
            _processFailedPersistedEventsWorker = new Worker("ProcessFailedPersistedEvents", ProcessFailedPersistedEvents);
        }

        #endregion

        #region Public Methods

        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }
        public void Start()
        {
            if (_enableGroupCommit)
            {
                _scheduleService.ScheduleTask("TryBatchPersistEvents", TryBatchPersistEvents, _groupCommitInterval, _groupCommitInterval);
                _processSuccessPersistedEventsWorker.Start();
                _processFailedPersistedEventsWorker.Start();
            }
        }
        public void CommitEventAsync(EventCommittingContext context)
        {
            if (_enableGroupCommit)
            {
                _toCommitContextQueue.Enqueue(context);
            }
            else
            {
                CommitEventAsync(context, 0);
            }
        }
        public void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStream eventStream)
        {
            if (eventStream.Items.Count == 0)
            {
                eventStream.Items = processingCommand.Items;
            }
            PublishDomainEventAsync(processingCommand, eventStream, 0);
        }
        public void PublishEventAsync(ProcessingCommand processingCommand, EventStream eventStream)
        {
            if (eventStream.Items.Count == 0)
            {
                eventStream.Items = processingCommand.Items;
            }
            PublishEventAsync(processingCommand, eventStream, 0);
        }

        #endregion

        #region Private Methods

        private bool EnterBatchPersistingEvents()
        {
            return Interlocked.CompareExchange(ref _isBatchPersistingEvents, 1, 0) == 0;
        }
        private void ExitBatchPersistingEvents()
        {
            Interlocked.Exchange(ref _isBatchPersistingEvents, 0);
        }
        private void TryBatchPersistEvents()
        {
            if (EnterBatchPersistingEvents())
            {
                var contextList = DequeueContexts();
                if (contextList.Count() > 0)
                {
                    BatchPersistEventsAsync(contextList, 0);
                }
                else
                {
                    ExitBatchPersistingEvents();
                }
            }
        }
        private void BatchPersistEventsAsync(IEnumerable<EventCommittingContext> contextList, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("BatchPersistEventAsync",
            () => _eventStore.BatchAppendAsync(contextList.Select(x => x.EventStream)),
            currentRetryTimes => BatchPersistEventsAsync(contextList, currentRetryTimes),
            result =>
            {
                _successPersistedContextQueue.Add(contextList);
                _logger.DebugFormat("Batch persist event stream success, persisted event stream count:{0}", contextList.Count());
                if (_toCommitContextQueue.Count >= _groupCommitMaxSize)
                {
                    BatchPersistEventsAsync(DequeueContexts(), 0);
                }
                else
                {
                    ExitBatchPersistingEvents();
                }
            },
            () => string.Format("[contextListCount:{0}]", contextList.Count()),
            () =>
            {
                _failedPersistedContextQueue.Add(contextList);
                ExitBatchPersistingEvents();
            },
            retryTimes);
        }
        private IEnumerable<EventCommittingContext> DequeueContexts()
        {
            var contextList = new List<EventCommittingContext>();
            EventCommittingContext context;

            while (contextList.Count < _groupCommitMaxSize && _toCommitContextQueue.TryDequeue(out context))
            {
                contextList.Add(context);
            }

            return contextList;
        }
        private void ProcessSuccessPersistedEvents()
        {
            foreach (var context in _successPersistedContextQueue.Take())
            {
                RefreshAggregateMemoryCache(context);
                PublishDomainEventAsync(context.ProcessingCommand, context.EventStream);
            }
        }
        private void ProcessFailedPersistedEvents()
        {
            foreach (var context in _failedPersistedContextQueue.Take())
            {
                CommitEventAsync(context, 0);
            }
        }

        private void CommitEventAsync(EventCommittingContext context, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<EventAppendResult>>("PersistEventAsync",
            () => _eventStore.AppendAsync(context.EventStream),
            currentRetryTimes => CommitEventAsync(context, currentRetryTimes),
            result =>
            {
                if (result.Data == EventAppendResult.Success)
                {
                    _logger.DebugFormat("Persist events success, {0}", context.EventStream);
                    RefreshAggregateMemoryCache(context);
                    PublishDomainEventAsync(context.ProcessingCommand, context.EventStream);
                }
                else if (result.Data == EventAppendResult.DuplicateEvent)
                {
                    HandleDuplicateEventResult(context);
                }
            },
            () => string.Format("[eventStream:{0}]", context.EventStream),
            () => context.ProcessingCommand.Complete(new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Command.Id, context.EventStream.AggregateRootId, null, "Persist event async failed.")),
            retryTimes);
        }
        private void HandleDuplicateEventResult(EventCommittingContext context)
        {
            //如果是当前事件的版本号为1，则认为是在创建重复的聚合根
            if (context.EventStream.Version == 1)
            {
                HandleFirstEventDuplicationAsync(context, 0);
            }
            //如果事件的版本大于1，则认为是更新聚合根时遇到并发冲突了；
            //那么我们需要先将聚合根的最新状态更新到内存，然后重试command；
            else
            {
                UpdateAggregateToLatestVersion(context.EventStream.AggregateRootTypeCode, context.EventStream.AggregateRootId);
                RetryConcurrentCommand(context);
            }
        }
        private void HandleFirstEventDuplicationAsync(EventCommittingContext context, int retryTimes)
        {
            var eventStream = context.EventStream;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<DomainEventStream>>("FindFirstEventByVersion",
            () => _eventStore.FindAsync(eventStream.AggregateRootId, 1),
            currentRetryTimes => HandleFirstEventDuplicationAsync(context, currentRetryTimes),
            result =>
            {
                var firstEventStream = result.Data;
                if (firstEventStream != null)
                {
                    //判断是否是同一个command，如果是，则再重新做一遍更新内存缓存以及发布事件这两个操作；
                    //之所以要这样做，是因为虽然该command产生的事件已经持久化成功，但并不表示已经内存也更新了或者事件已经发布出去了；
                    //有可能事件持久化成功了，但那时正好机器断电了，则更新内存和发布事件都没有做；
                    if (context.ProcessingCommand.Command.Id == firstEventStream.CommandId)
                    {
                        RefreshAggregateMemoryCache(firstEventStream);
                        PublishDomainEventAsync(context.ProcessingCommand, firstEventStream);
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
                        context.ProcessingCommand.Complete(new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Command.Id, eventStream.AggregateRootId, null, "Duplicate aggregate creation."));
                    }
                }
                else
                {
                    var errorMessage = string.Format("Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore. commandId:{0}, aggregateRootId:{1}, aggregateRootTypeCode:{2}",
                        eventStream.CommandId,
                        eventStream.AggregateRootId,
                        eventStream.AggregateRootTypeCode);
                    _logger.Error(errorMessage);
                    context.ProcessingCommand.Complete(new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Command.Id, eventStream.AggregateRootId, null, "Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore."));
                }
            },
            () => string.Format("[eventStream:{0}]", eventStream),
            () => context.ProcessingCommand.Complete(new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Command.Id, eventStream.AggregateRootId, null, "Persist the first version of event duplicated, but try to get the first version of domain event async failed.")),
            retryTimes);
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
                _logger.Error(string.Format("Refresh memory cache by aggregate first event stream failed, {0}", aggregateFirstEventStream), ex);
            }
        }
        private void RefreshAggregateMemoryCache(EventCommittingContext context)
        {
            try
            {
                context.AggregateRoot.AcceptChanges(context.EventStream.Version);
                _memoryCache.Set(context.AggregateRoot);
                _logger.DebugFormat("Refreshed aggregate memory cache, commandId:{0}, aggregateRootType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}", context.EventStream.CommandId, context.AggregateRoot.GetType().Name, context.AggregateRoot.UniqueId, context.AggregateRoot.Version);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Refresh memory cache failed by event stream:{0}", context.EventStream), ex);
            }
        }
        private void UpdateAggregateToLatestVersion(int aggregateRootTypeCode, string aggregateRootId)
        {
            _memoryCache.RefreshAggregateFromEventStore(aggregateRootTypeCode, aggregateRootId);
        }
        private void RetryConcurrentCommand(EventCommittingContext context)
        {
            var processingCommand = context.ProcessingCommand;
            var command = processingCommand.Command;
            processingCommand.IncreaseConcurrentRetriedCount();
            processingCommand.CommandExecuteContext.Clear();
            _logger.InfoFormat("Begin to retry command as it meets the concurrent conflict. commandType:{0}, commandId:{1}, aggregateRootId:{2}, retried count:{3}.", command.GetType().Name, command.Id, processingCommand.AggregateRootId, processingCommand.ConcurrentRetriedCount);
            _commandExecutor.ExecuteCommand(processingCommand);
        }
        private void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStream eventStream, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("PublishDomainEventAsync",
            () => _domainEventPublisher.PublishAsync(eventStream),
            currentRetryTimes => PublishDomainEventAsync(processingCommand, eventStream, currentRetryTimes),
            result =>
            {
                _logger.DebugFormat("Publish domain events success, {0}", eventStream);
                processingCommand.Complete(new CommandResult(CommandStatus.Success, processingCommand.Command.Id, eventStream.AggregateRootId, null, null));
            },
            () => string.Format("[eventStream:{0}]", eventStream),
            () => processingCommand.Complete(new CommandResult(CommandStatus.Failed, processingCommand.Command.Id, eventStream.AggregateRootId, null, "Publish domain event async failed.")),
            retryTimes);
        }
        private void PublishEventAsync(ProcessingCommand processingCommand, EventStream eventStream, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("PublishEventAsync",
            () => _eventPublisher.PublishAsync(eventStream),
            currentRetryTimes => PublishEventAsync(processingCommand, eventStream, currentRetryTimes),
            result =>
            {
                _logger.DebugFormat("Publish events success, {0}", eventStream);
                processingCommand.Complete(new CommandResult(CommandStatus.Success, processingCommand.Command.Id, null, null, null));
            },
            () => string.Format("[eventStream:{0}]", eventStream),
            () => processingCommand.Complete(new CommandResult(CommandStatus.Failed, processingCommand.Command.Id, null, null, "Publish event async failed.")),
            retryTimes);
        }

        #endregion
    }
}
