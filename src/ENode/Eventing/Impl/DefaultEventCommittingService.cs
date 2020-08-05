using System;
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
        private readonly IMemoryCache _memoryCache;
        private readonly IEventStore _eventStore;
        private readonly IMessagePublisher<DomainEventStreamMessage> _domainEventPublisher;
        private readonly IOHelper _ioHelper;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly int _batchSize;

        #endregion

        #region Constructors

        public DefaultEventCommittingService(
            IMemoryCache memoryCache,
            IEventStore eventStore,
            IMessagePublisher<DomainEventStreamMessage> domainEventPublisher,
            IOHelper ioHelper,
            IJsonSerializer jsonSerializer,
            ILoggerFactory loggerFactory)
        {
            _eventCommittingContextMailBoxList = new List<EventCommittingContextMailBox>();
            _ioHelper = ioHelper;
            _memoryCache = memoryCache;
            _eventStore = eventStore;
            _domainEventPublisher = domainEventPublisher;
            _jsonSerializer = jsonSerializer;
            _logger = loggerFactory.Create(GetType().FullName);
            _eventMailboxCount = ENodeConfiguration.Instance.Setting.EventMailBoxCount;
            _batchSize = ENodeConfiguration.Instance.Setting.EventMailBoxPersistenceMaxBatchSize;

            for (var i = 0; i < _eventMailboxCount; i++)
            {
                _eventCommittingContextMailBoxList.Add(new EventCommittingContextMailBox(i, _batchSize, x => BatchPersistEventAsync(x, 0), _jsonSerializer, _logger));
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
        private void BatchPersistEventAsync(IList<EventCommittingContext> committingContexts, int retryTimes)
        {
            if (committingContexts == null || committingContexts.Count == 0)
            {
                return;
            }

            _ioHelper.TryAsyncActionRecursively("BatchPersistEventAsync",
            () => _eventStore.BatchAppendAsync(committingContexts.Select(x => x.EventStream)),
            currentRetryTimes => BatchPersistEventAsync(committingContexts, currentRetryTimes),
            async result =>
            {
                var eventMailBox = committingContexts.First().MailBox;

                if (result == null)
                {
                    _logger.FatalFormat("BatchPersistAggregateEvents result is null, the current event committing mailbox should be pending, mailboxNumber: {0}", eventMailBox.Number);
                    return;
                }

                //针对持久化成功的聚合根，正常发布这些聚合根的事件到Q端
                if (result.SuccessAggregateRootIdList != null && result.SuccessAggregateRootIdList.Count > 0)
                {
                    foreach (var aggregateRootId in result.SuccessAggregateRootIdList)
                    {
                        var committingContextList = committingContexts.Where(x => x.EventStream.AggregateRootId == aggregateRootId).ToList();
                        if (committingContextList.Count > 0)
                        {
                            _logger.InfoFormat("BatchPersistAggregateEvents success, mailboxNumber: {0}, aggregateRootId: {1}, evnts: {2}",
                                eventMailBox.Number,
                                aggregateRootId,
                                string.Join(",", committingContextList.Select(x => x.EventStream.ToString())));

                            foreach (var committingContext in committingContextList)
                            {
                                PublishDomainEventAsync(committingContext.ProcessingCommand, committingContext.EventStream);
                            }
                        }
                    }
                }

                //针对持久化出现重复的命令ID，在命令MailBox中标记为已重复，在事件MailBox中清除对应聚合根产生的事件，且重新发布这些命令对应的领域事件到Q端
                if (result.DuplicateCommandAggregateRootIdList != null && result.DuplicateCommandAggregateRootIdList.Count > 0)
                {
                    foreach (var entry in result.DuplicateCommandAggregateRootIdList)
                    {
                        var committingContext = committingContexts.FirstOrDefault(x => x.EventStream.AggregateRootId == entry.Key);
                        if (committingContext != null)
                        {
                            _logger.WarnFormat("BatchPersistAggregateEvents has duplicate commandIds, mailboxNumber: {0}, aggregateRootId: {1}, commandIds: {2}",
                                eventMailBox.Number,
                                entry.Key,
                                string.Join(",", entry.Value));

                            if (committingContext.EventStream.Version == 1)
                            {
                                await HandleFirstEventDuplicationAsync(committingContext, 0);
                            }
                            else
                            {
                                await ResetCommandMailBoxConsumingSequence(committingContext, committingContext.ProcessingCommand.Sequence, entry.Value).ContinueWith(t => { }).ConfigureAwait(false);
                            }
                        }
                    }
                }

                //针对持久化出现版本冲突的聚合根，则自动处理每个聚合根的冲突
                if (result.DuplicateEventAggregateRootIdList != null && result.DuplicateEventAggregateRootIdList.Count > 0)
                {
                    foreach (var aggregateRootId in result.DuplicateEventAggregateRootIdList)
                    {
                        var committingContext = committingContexts.FirstOrDefault(x => x.EventStream.AggregateRootId == aggregateRootId);
                        if (committingContext != null)
                        {
                            _logger.WarnFormat("BatchPersistAggregateEvents has version confliction, mailboxNumber: {0}, aggregateRootId: {1}, conflictVersion: {2}",
                                eventMailBox.Number,
                                committingContext.EventStream.AggregateRootId,
                                committingContext.EventStream.Version);

                            if (committingContext.EventStream.Version == 1)
                            {
                                await HandleFirstEventDuplicationAsync(committingContext, 0);
                            }
                            else
                            {
                                await ResetCommandMailBoxConsumingSequence(committingContext, committingContext.ProcessingCommand.Sequence, null).ContinueWith(t => { }).ConfigureAwait(false);
                            }
                        }
                    }
                }

                committingContexts.First().MailBox.CompleteRun();
            },
            () => string.Format("[contextListCount:{0}]", committingContexts.Count),
            null,
            retryTimes, true);
        }
        private async Task ResetCommandMailBoxConsumingSequence(EventCommittingContext context, long consumingSequence, IList<string> duplicateCommandIdList)
        {
            var commandMailBox = context.ProcessingCommand.MailBox;
            var eventMailBox = context.MailBox;
            var aggregateRootId = context.EventStream.AggregateRootId;

            commandMailBox.Pause();

            try
            {
                eventMailBox.RemoveAggregateAllEventCommittingContexts(aggregateRootId);
                var refreshedAggregateRoot = await _memoryCache.RefreshAggregateFromEventStoreAsync(context.EventStream.AggregateRootTypeName, aggregateRootId).ConfigureAwait(false);
                if (duplicateCommandIdList != null)
                {
                    foreach (var commandId in duplicateCommandIdList)
                    {
                        commandMailBox.AddDuplicateCommandId(commandId);
                    }
                }
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
                var existingEventStream = result;
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
                    var errorMessage = string.Format("Command should be exist in the event store, but we cannot find it from the event store, this should not be happen, and we just complete the command. commandType:{0}, commandId:{1}, aggregateRootId:{2}",
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
        private Task HandleFirstEventDuplicationAsync(EventCommittingContext context, int retryTimes)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            _ioHelper.TryAsyncActionRecursively("FindFirstEventByVersion",
            () => _eventStore.FindAsync(context.EventStream.AggregateRootId, 1),
            currentRetryTimes => HandleFirstEventDuplicationAsync(context, currentRetryTimes),
            async result =>
            {
                if (result != null)
                {
                    var eventStream = result;
                    //判断是否是同一个command，如果是，则再重新做一遍发布事件；
                    //之所以要这样做，是因为虽然该command产生的事件已经持久化成功，但并不表示事件也已经发布出去了；
                    //有可能事件持久化成功了，但那时正好机器断电了，则发布事件都没有做；
                    if (context.ProcessingCommand.Message.Id == eventStream.CommandId)
                    {
                        await ResetCommandMailBoxConsumingSequence(context, context.ProcessingCommand.Sequence + 1, null).ConfigureAwait(false);
                        PublishDomainEventAsync(context.ProcessingCommand, eventStream);
                    }
                    else
                    {
                        //如果不是同一个command，则认为是两个不同的command重复创建ID相同的聚合根，我们需要记录错误日志，然后通知当前command的处理完成；
                        var errorMessage = string.Format("Duplicate aggregate creation. current commandId:{0}, existing commandId:{1}, aggregateRootId:{2}, aggregateRootTypeName:{3}",
                            context.ProcessingCommand.Message.Id,
                            eventStream.CommandId,
                            eventStream.AggregateRootId,
                            eventStream.AggregateRootTypeName);
                        _logger.Error(errorMessage);
                        await ResetCommandMailBoxConsumingSequence(context, context.ProcessingCommand.Sequence + 1, null).ConfigureAwait(false);
                        var commandResult = new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, context.EventStream.AggregateRootId, "Duplicate aggregate creation.", typeof(string).FullName);
                        await CompleteCommand(context.ProcessingCommand, commandResult);
                    }
                }
                else
                {
                    var errorMessage = string.Format("Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore, this should not be happen, and we just complete the command. commandId:{0}, aggregateRootId:{1}, aggregateRootTypeName:{2}",
                        context.EventStream.CommandId,
                        context.EventStream.AggregateRootId,
                        context.EventStream.AggregateRootTypeName);
                    _logger.Fatal(errorMessage);
                    await ResetCommandMailBoxConsumingSequence(context, context.ProcessingCommand.Sequence + 1, null).ConfigureAwait(false);
                    var commandResult = new CommandResult(CommandStatus.Failed, context.ProcessingCommand.Message.Id, context.EventStream.AggregateRootId, "Duplicate aggregate creation, but we cannot find the existing eventstream from eventstore.", typeof(string).FullName);
                    await CompleteCommand(context.ProcessingCommand, commandResult);
                }

                taskCompletionSource.SetResult(true);
            },
            () => string.Format("[eventStream:{0}]", context.EventStream),
            null,
            retryTimes, true);

            return taskCompletionSource.Task;
        }
        private void PublishDomainEventAsync(ProcessingCommand processingCommand, DomainEventStreamMessage eventStream, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursivelyWithoutResult("PublishEventAsync",
            () => _domainEventPublisher.PublishAsync(eventStream),
            currentRetryTimes => PublishDomainEventAsync(processingCommand, eventStream, currentRetryTimes),
            () =>
            {
                _logger.InfoFormat("Publish event success, {0}", eventStream);
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
