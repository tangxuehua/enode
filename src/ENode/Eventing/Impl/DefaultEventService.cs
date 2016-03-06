using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Scheduling;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class DefaultEventService : IEventService
    {
        #region Private Variables

        private IProcessingCommandHandler _processingCommandHandler;
        private readonly ConcurrentDictionary<string, EventMailBox> _eventMailboxDict;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IScheduleService _scheduleService;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly IEventStore _eventStore;
        private readonly IMessagePublisher<DomainEventStreamMessage> _domainEventPublisher;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly int _batchSize;

        #endregion

        #region Constructors

        public DefaultEventService(
            IJsonSerializer jsonSerializer,
            IScheduleService scheduleService,
            ITypeNameProvider typeNameProvider,
            IMemoryCache memoryCache,
            IAggregateRootFactory aggregateRootFactory,
            IAggregateStorage aggregateStorage,
            IEventStore eventStore,
            IMessagePublisher<DomainEventStreamMessage> domainEventPublisher,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            _eventMailboxDict = new ConcurrentDictionary<string, EventMailBox>();
            _ioHelper = ioHelper;
            _jsonSerializer = jsonSerializer;
            _scheduleService = scheduleService;
            _typeNameProvider = typeNameProvider;
            _memoryCache = memoryCache;
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateStorage = aggregateStorage;
            _eventStore = eventStore;
            _domainEventPublisher = domainEventPublisher;
            _logger = loggerFactory.Create(GetType().FullName);
            _batchSize = ENodeConfiguration.Instance.Setting.EventMailBoxPersistenceMaxBatchSize;
        }

        #endregion

        #region Public Methods

        public void SetProcessingCommandHandler(IProcessingCommandHandler processingCommandHandler)
        {
            _processingCommandHandler = processingCommandHandler;
        }
        public void CommitDomainEventAsync(EventCommittingContext context)
        {
            var eventMailbox = _eventMailboxDict.GetOrAdd(context.AggregateRoot.UniqueId, x =>
            {
                return new EventMailBox(x, _batchSize, eventMailBox =>
                {
                    if (_eventStore.SupportBatchAppendEvent)
                    {
                        BatchPersistEventAsync(eventMailBox, 0);
                    }
                    else
                    {
                        PersistEventOneByOne(eventMailBox.CommittingContexts);
                    }
                });
            });
            eventMailbox.EnqueueMessage(context);
            RefreshAggregateMemoryCache(context);
            context.ProcessingCommand.Mailbox.TryExecuteNextMessage();
        }
        public void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStream eventStream)
        {
            if (eventStream.Items == null || eventStream.Items.Count == 0)
            {
                eventStream.Items = processingCommand.Items;
            }
            var eventStreamMessage = new DomainEventStreamMessage(processingCommand.Message.Id, eventStream.AggregateRootId, eventStream.Version, eventStream.AggregateRootTypeName, eventStream.Events, eventStream.Items);
            PublishDomainEventAsync(processingCommand, eventStreamMessage, 0);
        }

        #endregion

        #region Private Methods

        private void BatchPersistEventAsync(EventMailBox eventMailBox, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("BatchPersistEventAsync",
            () => _eventStore.BatchAppendAsync(eventMailBox.CommittingContexts.Select(x => x.EventStream)),
            currentRetryTimes => BatchPersistEventAsync(eventMailBox, currentRetryTimes),
            result =>
            {
                var appendResult = result.Data;
                if (appendResult == EventAppendResult.Success)
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Batch persist event success, aggregateRootId: {0}, eventStreamCount: {1}", eventMailBox.AggregateRootId, eventMailBox.CommittingContexts.Count);
                    }

                    _logger.InfoFormat("Batch persist event success, aggregateRootId: {0}, eventStreamCount: {1}", eventMailBox.AggregateRootId, eventMailBox.CommittingContexts.Count);

                    eventMailBox
                        .CommittingContexts
                        .First()
                        .ProcessingCommand
                        .Mailbox
                        .RemoveCompleteMessages(eventMailBox.CommittingContexts.Select(x => x.ProcessingCommand));

                    Task.Factory.StartNew(x =>
                    {
                        var contextList = x as IList<EventCommittingContext>;
                        foreach (var context in contextList)
                        {
                            PublishDomainEventAsync(context.ProcessingCommand, context.EventStream);
                        }
                    }, new List<EventCommittingContext>(eventMailBox.CommittingContexts));

                    eventMailBox.RegisterForExecution(true);
                }
                else if (appendResult == EventAppendResult.DuplicateEvent)
                {
                    ProcessEventConcurrency(eventMailBox.CommittingContexts.First());
                }
                else if (appendResult == EventAppendResult.DuplicateCommand)
                {
                    PersistEventOneByOne(eventMailBox.CommittingContexts);
                }
            },
            () => string.Format("[contextListCount:{0}]", eventMailBox.CommittingContexts.Count),
            errorMessage =>
            {
                _logger.Error(errorMessage);
                //TODO
                //_failedPersistedContextQueue.Add(contextList);
                //_batchAppendLogger.WarnFormat("Batch persist event stream failed, event stream count:{0}", contextList.Count());
                //ExitBatchPersistingEvents();
            },
            retryTimes);
        }
        private void PersistEventOneByOne(IList<EventCommittingContext> contextList)
        {
            for (var i = 0; i < contextList.Count - 1; i++)
            {
                var currentContext = contextList[i];
                var nextContext = contextList[i + 1];
                currentContext.Next = nextContext;
            }
            PersistEventAsync(contextList[0], 0);
        }
        private void PersistEventAsync(EventCommittingContext context, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("PersistEventAsync",
            () => _eventStore.AppendAsync(context.EventStream),
            currentRetryTimes => PersistEventAsync(context, currentRetryTimes),
            result =>
            {
                if (result.Data == EventAppendResult.Success)
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Persist event success, {0}", context.EventStream);
                    }

                    context.ProcessingCommand.Mailbox.RemoveCompleteMessage(context.ProcessingCommand);

                    Task.Factory.StartNew(x =>
                    {
                        var currentContext = x as EventCommittingContext;
                        PublishDomainEventAsync(currentContext.ProcessingCommand, currentContext.EventStream);
                    }, context);

                    TryProcessNextContext(context);
                }
                else if (result.Data == EventAppendResult.DuplicateEvent)
                {
                    //如果是当前事件的版本号为1，则认为是在创建重复的聚合根
                    if (context.EventStream.Version == 1)
                    {
                        context.ProcessingCommand.Mailbox.RemoveCompleteMessage(context.ProcessingCommand);
                        HandleFirstEventDuplicationAsync(context, 0);
                        TryProcessNextContext(context);
                    }
                    //如果事件的版本大于1，则认为是更新聚合根时遇到并发冲突了，则需要进行重试；
                    else
                    {
                        ProcessEventConcurrency(context);
                    }
                }
                else if (result.Data == EventAppendResult.DuplicateCommand)
                {
                    HandleDuplicateCommandResult(context, 0);
                    TryProcessNextContext(context);
                }
            },
            () => string.Format("[eventStream:{0}]", context.EventStream),
            errorMessage => SetCommandResult(context.ProcessingCommand, new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, context.EventStream.AggregateRootId, errorMessage ?? "Persist event async failed.", typeof(string).FullName)),
            retryTimes);
        }
        private void ProcessEventConcurrency(EventCommittingContext context)
        {
            _logger.WarnFormat("Persist event has concurrent conflict, eventStream: {0}", context.EventStream);

            var eventMailBox = context.EventMailBox;
            var processingCommand = context.ProcessingCommand;
            var commandMailBox = processingCommand.Mailbox;

            commandMailBox.StopHandlingMessage();
            UpdateAggregateToLatestVersion(context.EventStream.AggregateRootTypeName, eventMailBox.AggregateRootId);
            commandMailBox.ResetConsumingOffset(processingCommand.Sequence);
            eventMailBox.Clear();
            eventMailBox.ExitHandlingMessage();
            commandMailBox.RestartHandlingMessage();
        }
        private void HandleDuplicateCommandResult(EventCommittingContext context, int retryTimes)
        {
            var command = context.ProcessingCommand.Message;

            _ioHelper.TryAsyncActionRecursively("FindEventByCommandIdAsync",
            () => _eventStore.FindAsync(command.AggregateRootId, command.Id),
            currentRetryTimes => HandleDuplicateCommandResult(context, currentRetryTimes),
            result =>
            {
                var existingEventStream = result.Data;
                if (existingEventStream != null)
                {
                    //这里，我们需要再重新做一遍发布事件，确保最终一致性；
                    //之所以要这样做是因为虽然该command产生的事件已经持久化成功，但并不表示事件已经发布出去了；
                    //因为有可能事件持久化成功了，但那时正好机器断电了，则发布事件都没有做；
                    PublishDomainEventAsync(context.ProcessingCommand, existingEventStream);
                }
                else
                {
                    //到这里，说明当前command想添加到eventStore中时，提示command重复，但是尝试从eventStore中取出该command时却找不到该command。
                    //出现这种情况，我们就无法再做后续处理了，这种错误理论上不会出现，除非eventStore的Add接口和Get接口出现读写不一致的情况；
                    //我们记录错误日志，然后认为当前command已被处理为失败。
                    var errorMessage = string.Format("Command exist in the event store, but we cannot get it from the event store. commandType:{0}, commandId:{1}, aggregateRootId:{2}",
                        command.GetType().Name,
                        command.Id,
                        command.AggregateRootId);
                    _logger.Error(errorMessage);
                    SetCommandResult(context.ProcessingCommand, new CommandResult(CommandStatus.Failed, command.Id, command.AggregateRootId, "Duplicate command execution.", typeof(string).FullName));
                }
            },
            () => string.Format("[aggregateRootId:{0}, commandId:{1}]", command.AggregateRootId, command.Id),
            errorMessage => SetCommandResult(context.ProcessingCommand, new CommandResult(CommandStatus.Failed, command.Id, command.AggregateRootId, "Find event by commandId failed.", typeof(string).FullName)),
            retryTimes);
        }
        private void HandleFirstEventDuplicationAsync(EventCommittingContext context, int retryTimes)
        {
            var eventStream = context.EventStream;

            _ioHelper.TryAsyncActionRecursively("FindFirstEventByVersion",
            () => _eventStore.FindAsync(eventStream.AggregateRootId, 1),
            currentRetryTimes => HandleFirstEventDuplicationAsync(context, currentRetryTimes),
            result =>
            {
                var firstEventStream = result.Data;
                if (firstEventStream != null)
                {
                    //判断是否是同一个command，如果是，则再重新做一遍发布事件；
                    //之所以要这样做，是因为虽然该command产生的事件已经持久化成功，但并不表示事件也已经发布出去了；
                    //有可能事件持久化成功了，但那时正好机器断电了，则发布事件都没有做；
                    if (context.ProcessingCommand.Message.Id == firstEventStream.CommandId)
                    {
                        PublishDomainEventAsync(context.ProcessingCommand, firstEventStream);
                    }
                    else
                    {
                        //如果不是同一个command，则认为是两个不同的command重复创建ID相同的聚合根，我们需要记录错误日志，然后通知当前command的处理完成；
                        var errorMessage = string.Format("Duplicate aggregate creation. current commandId:{0}, existing commandId:{1}, aggregateRootId:{2}, aggregateRootTypeName:{3}",
                            context.ProcessingCommand.Message.Id,
                            firstEventStream.CommandId,
                            firstEventStream.AggregateRootId,
                            firstEventStream.AggregateRootTypeName);
                        _logger.Error(errorMessage);
                        SetCommandResult(context.ProcessingCommand, new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, eventStream.AggregateRootId, "Duplicate aggregate creation.", typeof(string).FullName));
                    }
                }
                else
                {
                    var errorMessage = string.Format("Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore. commandId:{0}, aggregateRootId:{1}, aggregateRootTypeName:{2}",
                        eventStream.CommandId,
                        eventStream.AggregateRootId,
                        eventStream.AggregateRootTypeName);
                    _logger.Error(errorMessage);
                    SetCommandResult(context.ProcessingCommand, new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, eventStream.AggregateRootId, "Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore.", typeof(string).FullName));
                }
            },
            () => string.Format("[eventStream:{0}]", eventStream),
            errorMessage => SetCommandResult(context.ProcessingCommand, new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, eventStream.AggregateRootId, errorMessage ?? "Persist the first version of event duplicated, but try to get the first version of domain event async failed.", typeof(string).FullName)),
            retryTimes);
        }
        private void RefreshAggregateMemoryCache(DomainEventStream aggregateFirstEventStream)
        {
            try
            {
                var aggregateRootType = _typeNameProvider.GetType(aggregateFirstEventStream.AggregateRootTypeName);
                var aggregateRoot = _memoryCache.Get(aggregateFirstEventStream.AggregateRootId, aggregateRootType);
                if (aggregateRoot == null)
                {
                    aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                    aggregateRoot.ReplayEvents(new DomainEventStream[] { aggregateFirstEventStream });
                    _memoryCache.Set(aggregateRoot);
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Aggregate added into memory, commandId:{0}, aggregateRootType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}", aggregateFirstEventStream.CommandId, aggregateRootType.Name, aggregateRoot.UniqueId, aggregateRoot.Version);
                    }
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
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Refreshed aggregate memory cache, commandId:{0}, aggregateRootType:{1}, aggregateRootId:{2}, version:{3}", context.EventStream.CommandId, context.AggregateRoot.GetType().Name, context.AggregateRoot.UniqueId, context.AggregateRoot.Version);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Refresh memory cache failed for event stream:{0}", context.EventStream), ex);
            }
        }
        private void UpdateAggregateToLatestVersion(string aggregateRootTypeName, string aggregateRootId)
        {
            _memoryCache.RefreshAggregateFromEventStore(aggregateRootTypeName, aggregateRootId);
        }
        private void TryProcessNextContext(EventCommittingContext currentContext)
        {
            if (currentContext.Next != null)
            {
                PersistEventAsync(currentContext.Next, 0);
            }
            else
            {
                currentContext.EventMailBox.RegisterForExecution(true);
            }
        }
        private void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStreamMessage eventStream, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("PublishEventAsync",
            () => _domainEventPublisher.PublishAsync(eventStream),
            currentRetryTimes => PublishDomainEventAsync(processingCommand, eventStream, currentRetryTimes),
            result =>
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Publish event success, {0}", eventStream);
                }
                var commandHandleResult = processingCommand.CommandExecuteContext.GetResult();
                SetCommandResult(processingCommand, new CommandResult(CommandStatus.Success, processingCommand.Message.Id, eventStream.AggregateRootId, commandHandleResult, typeof(string).FullName));
            },
            () => string.Format("[eventStream:{0}]", eventStream),
            errorMessage => SetCommandResult(processingCommand, new CommandResult(CommandStatus.Failed, processingCommand.Message.Id, eventStream.AggregateRootId, errorMessage ?? "Publish domain event async failed.", typeof(string).FullName)),
            retryTimes);
        }
        private void SetCommandResult(ProcessingCommand processingCommand, CommandResult commandResult)
        {
            processingCommand.SetResult(commandResult);
        }

        #endregion
    }
}
