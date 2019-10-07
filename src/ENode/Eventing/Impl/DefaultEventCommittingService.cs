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
using ENode.Messaging;

namespace ENode.Eventing.Impl
{
    public class DefaultEventCommittingService : IEventCommittingService
    {
        #region Private Variables

        private readonly int _eventMailboxCount;
        private readonly IList<EventCommittingContextMailBox> _eventCommittingContextMailBoxList;
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

        public DefaultEventCommittingService(
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
            _eventCommittingContextMailBoxList = new List<EventCommittingContextMailBox>();
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
            _eventMailboxCount = ENodeConfiguration.Instance.Setting.EventMailBoxCount;
            _batchSize = ENodeConfiguration.Instance.Setting.EventMailBoxPersistenceMaxBatchSize;

            for (var i = 0; i < _eventMailboxCount; i++)
            {
                _eventCommittingContextMailBoxList.Add(new EventCommittingContextMailBox(i, _batchSize, BatchPersistEventCommittingContexts, _logger));
            }
        }

        #endregion

        #region Public Methods

        public void CommitDomainEventAsync(EventCommittingContext eventCommittingContext)
        {
            var eventMailboxIndex = GetEventMailBoxIndex(eventCommittingContext.EventStream.AggregateRootId);
            var eventMailbox = _eventCommittingContextMailBoxList[eventMailboxIndex];
            eventMailbox.EnqueueMessage(eventCommittingContext);
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

        private int GetEventMailBoxIndex(string aggregateRootId)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in aggregateRootId)
                {
                    hash = (hash << 5) - hash + c;
                }
                if (hash < 0)
                {
                    hash = Math.Abs(hash);
                }
                return hash % _eventMailboxCount;
            }
        }
        private void BatchPersistEventCommittingContexts(IList<EventCommittingContext> committingContexts)
        {
            if (committingContexts == null || committingContexts.Count == 0)
            {
                return;
            }
            BatchPersistEventAsync(committingContexts, 0);
        }
        private void BatchPersistEventAsync(IList<EventCommittingContext> committingContexts, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("BatchPersistEventAsync",
            () => _eventStore.BatchAppendAsync(committingContexts.Select(x => x.EventStream)),
            currentRetryTimes => BatchPersistEventAsync(committingContexts, currentRetryTimes),
            result =>
            {
                var eventMailBox = committingContexts.First().MailBox;
                var appendResult = result.Data;

                //针对持久化成功的聚合根，发布这些聚合根的事件到Q端
                if (appendResult.SuccessAggregateRootIdList != null && appendResult.SuccessAggregateRootIdList.Count > 0)
                {
                    var successCommittedContextDict = new Dictionary<string, IList<EventCommittingContext>>();
                    foreach (var aggregateRootId in appendResult.SuccessAggregateRootIdList)
                    {
                        var contextList = committingContexts.Where(x => x.EventStream.AggregateRootId == aggregateRootId).ToList();
                        if (contextList.Count > 0)
                        {
                            successCommittedContextDict.Add(aggregateRootId, contextList);
                        }
                    }
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Batch persist events, mailboxNumber: {0}, succeedAggregateRootCount: {1}, eventStreamDetail: {2}",
                            eventMailBox.Number,
                            appendResult.SuccessAggregateRootIdList.Count,
                            _jsonSerializer.Serialize(successCommittedContextDict));
                    }

                    Task.Factory.StartNew(x =>
                    {
                        var contextListDict = x as Dictionary<string, IList<EventCommittingContext>>;
                        foreach (var entry in contextListDict)
                        {
                            foreach (var committingContext in entry.Value)
                            {
                                PublishDomainEventAsync(committingContext.ProcessingCommand, committingContext.EventStream);
                            }
                        }
                    }, successCommittedContextDict);
                }

                //针对持久化出现重复的命令ID，则重新发布这些命令对应的领域事件到Q端
                if (appendResult.DuplicateCommandIdList != null && appendResult.DuplicateCommandIdList.Count > 0)
                {
                    _logger.WarnFormat("Batch persist events, mailboxNumber: {0}, duplicateCommandIdCount: {1}, detail: {2}",
                        eventMailBox.Number,
                        appendResult.DuplicateCommandIdList.Count,
                        _jsonSerializer.Serialize(appendResult.DuplicateCommandIdList));

                    foreach (var commandId in appendResult.DuplicateCommandIdList)
                    {
                        var committingContext = committingContexts.FirstOrDefault(x => x.ProcessingCommand.Message.Id == commandId);
                        if (committingContext != null)
                        {
                            ResetCommandMailBoxConsumingSequence(committingContext, committingContext.ProcessingCommand.Sequence + 1).ContinueWith(t =>
                            {
                                TryToRepublishEventAsync(committingContext, 0);
                            }).ConfigureAwait(false);
                        }
                    }
                }

                //针对持久化出现版本冲突的聚合根，则自动处理每个聚合根的冲突
                if (appendResult.DuplicateEventAggregateRootIdList != null && appendResult.DuplicateEventAggregateRootIdList.Count > 0)
                {
                    _logger.WarnFormat("Batch persist events, mailboxNumber: {0}, duplicateEventAggregateRootCount: {1}, detail: {2}",
                        eventMailBox.Number,
                        appendResult.DuplicateEventAggregateRootIdList.Count,
                        _jsonSerializer.Serialize(appendResult.DuplicateEventAggregateRootIdList));

                    foreach (var aggregateRootId in appendResult.DuplicateEventAggregateRootIdList)
                    {
                        var committingContext = committingContexts.FirstOrDefault(x => x.EventStream.AggregateRootId == aggregateRootId);
                        if (committingContext != null)
                        {
                            ProcessAggregateDuplicateEvent(committingContext);
                        }
                    }
                }

                //最终，将当前的EventMailBox的本次处理标记为处理完成，然后继续可以处理下一批事件
                eventMailBox.CompleteRun();
            },
            () => string.Format("[contextListCount:{0}]", committingContexts.Count),
            null,
            retryTimes, true);
        }
        private void ProcessAggregateDuplicateEvent(EventCommittingContext eventCommittingContext)
        {
            if (eventCommittingContext.EventStream.Version == 1)
            {
                HandleFirstEventDuplicationAsync(eventCommittingContext, 0);
            }
            else
            {
                ResetCommandMailBoxConsumingSequence(eventCommittingContext, eventCommittingContext.ProcessingCommand.Sequence).ContinueWith(t => { }).ConfigureAwait(false);
            }
        }
        private async Task ResetCommandMailBoxConsumingSequence(EventCommittingContext context, long consumingSequence)
        {
            var commandMailBox = context.ProcessingCommand.MailBox;
            var eventMailBox = context.MailBox;
            var aggregateRootId = context.EventStream.AggregateRootId;

            commandMailBox.Pause();

            try
            {
                eventMailBox.RemoveAggregateAllEventCommittingContexts(aggregateRootId);
                var refreshedAggregateRoot = await _memoryCache.RefreshAggregateFromEventStoreAsync(context.EventStream.AggregateRootTypeName, aggregateRootId).ConfigureAwait(false);
                commandMailBox.ResetConsumingSequence(consumingSequence);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("ResetCommandMailBoxConsumingSequence has unknown exception, aggregateRootId: {0}", aggregateRootId), ex);
            }
            finally
            {
                commandMailBox.Resume();
                commandMailBox.TryRun();
            }
        }
        private void TryToRepublishEventAsync(EventCommittingContext context, int retryTimes)
        {
            var command = context.ProcessingCommand.Message;

            _ioHelper.TryAsyncActionRecursively("FindEventByCommandIdAsync",
            () => _eventStore.FindAsync(command.AggregateRootId, command.Id),
            currentRetryTimes => TryToRepublishEventAsync(context, currentRetryTimes),
            result =>
            {
                var existingEventStream = result.Data;
                if (existingEventStream != null)
                {
                    //这里，我们需要再重新做一遍发布事件这个操作；
                    //之所以要这样做是因为虽然该command产生的事件已经持久化成功，但并不表示事件已经发布出去了；
                    //因为有可能事件持久化成功了，但那时正好机器断电了，则发布事件都没有做；
                    PublishDomainEventAsync(context.ProcessingCommand, existingEventStream);
                }
                else
                {
                    //到这里，说明当前command想添加到eventStore中时，提示command重复，但是尝试从eventStore中取出该command时却找不到该command。
                    //出现这种情况，我们就无法再做后续处理了，这种错误理论上不会出现，除非eventStore的Add接口和Get接口出现读写不一致的情况；
                    //框架会记录错误日志，让开发者排查具体是什么问题。
                    var errorMessage = string.Format("Command should be exist in the event store, but we cannot find it from the event store, this should not be happen, and we cannot continue again. commandType:{0}, commandId:{1}, aggregateRootId:{2}",
                        command.GetType().Name,
                        command.Id,
                        command.AggregateRootId);
                    _logger.Fatal(errorMessage);
                    var commandResult = new CommandResult(CommandStatus.Failed, command.Id, command.AggregateRootId, "Command should be exist in the event store, but we cannot find it from the event store.", typeof(string).FullName);
                    CompleteCommand(context.ProcessingCommand, commandResult);
                }
            },
            () => string.Format("[aggregateRootId:{0}, commandId:{1}]", command.AggregateRootId, command.Id),
            null,
            retryTimes, true);
        }
        private void HandleFirstEventDuplicationAsync(EventCommittingContext context, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("FindFirstEventByVersion",
            () => _eventStore.FindAsync(context.EventStream.AggregateRootId, 1),
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
                        ResetCommandMailBoxConsumingSequence(context, context.ProcessingCommand.Sequence + 1).ContinueWith(t =>
                        {
                            PublishDomainEventAsync(context.ProcessingCommand, firstEventStream);
                        }).ConfigureAwait(false);
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
                        ResetCommandMailBoxConsumingSequence(context, context.ProcessingCommand.Sequence + 1).ContinueWith(t =>
                        {
                            var commandResult = new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, context.EventStream.AggregateRootId, "Duplicate aggregate creation.", typeof(string).FullName);
                            CompleteCommand(context.ProcessingCommand, commandResult);
                        }).ConfigureAwait(false);
                    }
                }
                else
                {
                    var errorMessage = string.Format("Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore, this should not be happen, and we cannot continue again. commandId:{0}, aggregateRootId:{1}, aggregateRootTypeName:{2}",
                        context.EventStream.CommandId,
                        context.EventStream.AggregateRootId,
                        context.EventStream.AggregateRootTypeName);
                    _logger.Fatal(errorMessage);
                    ResetCommandMailBoxConsumingSequence(context, context.ProcessingCommand.Sequence + 1).ContinueWith(t =>
                    {
                        var commandResult = new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, context.EventStream.AggregateRootId, "Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore.", typeof(string).FullName);
                        CompleteCommand(context.ProcessingCommand, commandResult);
                    }).ConfigureAwait(false);
                }
            },
            () => string.Format("[eventStream:{0}]", context.EventStream),
            null,
            retryTimes, true);
        }
        private void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStreamMessage eventStream, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("PublishEventAsync",
            () => _domainEventPublisher.PublishAsync(eventStream),
            currentRetryTimes => PublishDomainEventAsync(processingCommand, eventStream, currentRetryTimes),
            result =>
            {
                _logger.DebugFormat("Publish event success, {0}", eventStream);
                var commandHandleResult = processingCommand.CommandExecuteContext.GetResult();
                var commandResult = new CommandResult(CommandStatus.Success, processingCommand.Message.Id, eventStream.AggregateRootId, commandHandleResult, typeof(string).FullName);
                CompleteCommand(processingCommand, commandResult);
            },
            () => string.Format("[eventStream:{0}]", eventStream),
            null,
            retryTimes, true);
        }
        private Task CompleteCommand(ProcessingCommand processingCommand, CommandResult commandResult)
        {
            return processingCommand.MailBox.CompleteMessage(processingCommand, commandResult);
        }

        #endregion
    }
}
