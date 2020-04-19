using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.Messaging;

namespace ENode.Eventing.Impl
{
    public class DefaultProcessingEventProcessor : IProcessingEventProcessor
    {
        private readonly ConcurrentDictionary<string, Lazy<Task<ProcessingEventMailBox>>> _mailboxDict;
        private readonly IPublishedVersionStore _publishedVersionStore;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly IScheduleService _scheduleService;
        private readonly int _timeoutSeconds;
        private readonly string _scanInactiveMailBoxTaskName;
        private readonly string _processProblemAggregateTaskName;
        private readonly int _scanExpiredAggregateIntervalMilliseconds;
        private readonly int _processProblemAggregateIntervalMilliseconds;
        private readonly ConcurrentDictionary<string, ProcessingEventMailBox> _problemAggregateRootMailBoxDict;

        public string Name { get; }

        public DefaultProcessingEventProcessor(IPublishedVersionStore publishedVersionStore, IMessageDispatcher dispatcher, IOHelper ioHelper, ILoggerFactory loggerFactory, IScheduleService scheduleService)
        {
            _mailboxDict = new ConcurrentDictionary<string, Lazy<Task<ProcessingEventMailBox>>>();
            _problemAggregateRootMailBoxDict = new ConcurrentDictionary<string, ProcessingEventMailBox>();
            _publishedVersionStore = publishedVersionStore;
            _dispatcher = dispatcher;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
            _scheduleService = scheduleService;
            _scanInactiveMailBoxTaskName = "CleanInactiveProcessingEventMailBoxes_" + DateTime.Now.Ticks + new Random().Next(10000);
            _processProblemAggregateTaskName = "ProcessProblemAggregate_" + DateTime.Now.Ticks + new Random().Next(10000);
            _timeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            Name = ENodeConfiguration.Instance.Setting.DomainEventProcessorName;
            _scanExpiredAggregateIntervalMilliseconds = ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds;
            _processProblemAggregateIntervalMilliseconds = ENodeConfiguration.Instance.Setting.ProcessProblemAggregateIntervalMilliseconds;
        }

        public async Task ProcessAsync(ProcessingEvent processingMessage)
        {
            var aggregateRootId = processingMessage.Message.AggregateRootId;
            if (string.IsNullOrEmpty(aggregateRootId))
            {
                throw new ArgumentException("aggregateRootId of domain event stream cannot be null or empty, domainEventStreamId:" + processingMessage.Message.Id);
            }
            var mailbox = await _mailboxDict.GetOrAdd(
                aggregateRootId,
                x => new Lazy<Task<ProcessingEventMailBox>>(() => BuildProcessingEventMailBoxAsync(processingMessage))).Value;
            var mailboxTryUsingCount = 0L;
            while (!mailbox.TryUsing())
            {
                //todo: consider use Task.Yield() to release ThreadPool task thread.
                await Task.Delay(1);
                mailboxTryUsingCount++;
                if (mailboxTryUsingCount % 10000 == 0)
                {
                    _logger.WarnFormat("Event mailbox try using count: {0}, aggregateRootId: {1}, aggregateRootTypeName: {2}", mailboxTryUsingCount, mailbox.AggregateRootId, mailbox.AggregateRootTypeName);
                }
            }
            if (mailbox.IsRemoved)
            {
                var mailboxTask = new Lazy<Task<ProcessingEventMailBox>>(() => BuildProcessingEventMailBoxAsync(processingMessage));
                _mailboxDict.TryAdd(aggregateRootId, mailboxTask);
                mailbox = await mailboxTask.Value;
            }
            ProcessingEventMailBox.EnqueueMessageResult enqueueResult = mailbox.EnqueueMessage(processingMessage);
            if (enqueueResult == ProcessingEventMailBox.EnqueueMessageResult.Ignored)
            {
                processingMessage.ProcessContext.NotifyEventProcessed();
            }
            else if (enqueueResult == ProcessingEventMailBox.EnqueueMessageResult.AddToWaitingList)
            {
                AddProblemAggregateMailBoxToDict(mailbox);
            }
            mailbox.ExitUsing();
        }
        public void Start()
        {
            _scheduleService.StartTask(_scanInactiveMailBoxTaskName, async () => await CleanInactiveMailboxAsync(), _scanExpiredAggregateIntervalMilliseconds, _scanExpiredAggregateIntervalMilliseconds);
            _scheduleService.StartTask(_processProblemAggregateTaskName, async () => await ProcessProblemAggregatesAsync(), _processProblemAggregateIntervalMilliseconds, _processProblemAggregateIntervalMilliseconds);
        }
        public void Stop()
        {
            _scheduleService.StopTask(_scanInactiveMailBoxTaskName);
        }

        private async Task<ProcessingEventMailBox> BuildProcessingEventMailBoxAsync(ProcessingEvent processingMessage)
        {
            var latestHandledEventVersion = await GetAggregateRootLatestHandledEventVersionAsync(processingMessage.Message.AggregateRootTypeName, processingMessage.Message.AggregateRootId);
            return new ProcessingEventMailBox(processingMessage.Message.AggregateRootTypeName, processingMessage.Message.AggregateRootId, latestHandledEventVersion + 1, y => DispatchProcessingMessageAsync(y, 0), _logger);
        }
        private void AddProblemAggregateMailBoxToDict(ProcessingEventMailBox mailbox)
        {
            if (_problemAggregateRootMailBoxDict.TryAdd(mailbox.AggregateRootId, mailbox))
            {
                _logger.WarnFormat("Added problem aggregate mailbox, aggregateRootTypeName: {0}, aggregateRootId: {1}", mailbox.AggregateRootTypeName, mailbox.AggregateRootId);
            }
        }
        private async Task ProcessProblemAggregatesAsync()
        {
            var entryList = _problemAggregateRootMailBoxDict.ToArray();
            if (entryList.Length == 0)
            {
                return;
            }
            var problemMailboxList = new List<ProcessingEventMailBox>();
            var recoveredMailboxList = new List<ProcessingEventMailBox>();
            foreach (var entry in entryList)
            {
                var aggregateRootMailBox = entry.Value;
                if (aggregateRootMailBox.WaitingMessageCount > 0)
                {
                    problemMailboxList.Add(aggregateRootMailBox);
                }
                else
                {
                    recoveredMailboxList.Add(aggregateRootMailBox);
                }
            }
            foreach (var mailbox in problemMailboxList)
            {
                var latestHandledEventVersion = await GetAggregateRootLatestHandledEventVersionAsync(mailbox.AggregateRootTypeName, mailbox.AggregateRootId);
                mailbox.SetNextExpectingEventVersion(latestHandledEventVersion + 1);
            }
            foreach (var mailbox in recoveredMailboxList)
            {
                if (_problemAggregateRootMailBoxDict.TryRemove(mailbox.AggregateRootId, out ProcessingEventMailBox removed))
                {
                    _logger.InfoFormat("Removed problem aggregate mailbox, aggregateRootTypeName: {0}, aggregateRootId: {1}", removed.AggregateRootTypeName, removed.AggregateRootId);
                }
            }
        }
        private void DispatchProcessingMessageAsync(ProcessingEvent processingMessage, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursivelyWithoutResult("DispatchProcessingMessageAsync",
            () => _dispatcher.DispatchMessagesAsync(processingMessage.Message.Events),
            currentRetryTimes => DispatchProcessingMessageAsync(processingMessage, currentRetryTimes),
            () =>
            {
                UpdatePublishedVersionAsync(processingMessage, 0);
            },
            () => string.Format("Message[messageId:{0}, messageType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}]", processingMessage.Message.Id, processingMessage.Message.GetType().Name, processingMessage.Message.AggregateRootId, processingMessage.Message.Version),
            null,
            retryTimes, true);
        }
        private async Task<int> GetAggregateRootLatestHandledEventVersionAsync(string aggregateRootTypeName, string aggregateRootId)
        {
            try
            {
                return await _publishedVersionStore.GetPublishedVersionAsync(Name, aggregateRootTypeName, aggregateRootId);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("_publishedVersionStore.GetPublishedVersionAsync has unknown exception, aggregateRootTypeName: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId), ex);
            }
        }
        private void UpdatePublishedVersionAsync(ProcessingEvent processingMessage, int retryTimes)
        {
            var message = processingMessage.Message;
            _ioHelper.TryAsyncActionRecursivelyWithoutResult("UpdatePublishedVersionAsync",
            () => _publishedVersionStore.UpdatePublishedVersionAsync(Name, message.AggregateRootTypeName, message.AggregateRootId, message.Version),
            currentRetryTimes => UpdatePublishedVersionAsync(processingMessage, currentRetryTimes),
            () =>
            {
                processingMessage.Complete();
            },
            () => string.Format("DomainEventStreamMessage [messageId:{0}, messageType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}]", message.Id, message.GetType().Name, message.AggregateRootId, message.Version),
            null,
            retryTimes, true);
        }
        private async Task CleanInactiveMailboxAsync()
        {
            var inactiveList = new List<KeyValuePair<string, Lazy<Task<ProcessingEventMailBox>>>>();
            foreach (var pair in _mailboxDict)
            {
                if (IsMailBoxAllowRemove(await pair.Value.Value))
                {
                    inactiveList.Add(pair);
                }
            }
            foreach (var pair in inactiveList)
            {
                var mailbox = await pair.Value.Value;
                if (mailbox.TryUsing())
                {
                    if (IsMailBoxAllowRemove(mailbox))
                    {
                        if (_mailboxDict.TryRemove(pair.Key, out Lazy<Task<ProcessingEventMailBox>> removedTask))
                        {
                            var removed = await removedTask.Value;
                            removed.MarkAsRemoved();
                            _logger.InfoFormat("Removed inactive domain event stream mailbox, aggregateRootTypeName: {0}, aggregateRootId: {1}", removed.AggregateRootTypeName, removed.AggregateRootId);
                        }
                    }
                }
                mailbox.ExitUsing();
            }
        }
        private bool IsMailBoxAllowRemove(ProcessingEventMailBox mailbox)
        {
            return mailbox.IsInactive(_timeoutSeconds) && !mailbox.IsRunning && mailbox.TotalUnHandledMessageCount == 0 && mailbox.WaitingMessageCount == 0;
        }
    }
}
