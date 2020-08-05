using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Scheduling;
using ECommon.Serializing;
using ENode.Configurations;
using ENode.Messaging;

namespace ENode.Eventing.Impl
{
    public class DefaultProcessingEventProcessor : IProcessingEventProcessor
    {
        private readonly object _lockObj = new object();
        private readonly ConcurrentDictionary<string, ProcessingEventMailBox> _mailboxDict;
        private readonly IPublishedVersionStore _publishedVersionStore;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IOHelper _ioHelper;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly IScheduleService _scheduleService;
        private readonly int _timeoutSeconds;
        private readonly string _scanInactiveMailBoxTaskName;
        private readonly string _processTryToRefreshAggregateTaskName;
        private readonly int _scanExpiredAggregateIntervalMilliseconds;
        private readonly int _processTryToRefreshAggregateIntervalMilliseconds;
        private readonly ConcurrentDictionary<string, ProcessingEventMailBox> _toRefreshAggregateRootMailBoxDict;
        private readonly ConcurrentDictionary<string, bool> _refreshingAggregateRootDict;

        public string Name { get; }

        public DefaultProcessingEventProcessor(IPublishedVersionStore publishedVersionStore, IMessageDispatcher dispatcher, IOHelper ioHelper, IJsonSerializer jsonSerializer, ILoggerFactory loggerFactory, IScheduleService scheduleService)
        {
            _mailboxDict = new ConcurrentDictionary<string, ProcessingEventMailBox>();
            _toRefreshAggregateRootMailBoxDict = new ConcurrentDictionary<string, ProcessingEventMailBox>();
            _refreshingAggregateRootDict = new ConcurrentDictionary<string, bool>();
            _publishedVersionStore = publishedVersionStore;
            _dispatcher = dispatcher;
            _ioHelper = ioHelper;
            _jsonSerializer = jsonSerializer;
            _logger = loggerFactory.Create(GetType().FullName);
            _scheduleService = scheduleService;
            _scanInactiveMailBoxTaskName = "CleanInactiveProcessingEventMailBoxes_" + DateTime.Now.Ticks + new Random().Next(10000);
            _processTryToRefreshAggregateTaskName = "ProcessTryToRefreshAggregate_" + DateTime.Now.Ticks + new Random().Next(10000);
            _timeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            Name = ENodeConfiguration.Instance.Setting.DomainEventProcessorName;
            _scanExpiredAggregateIntervalMilliseconds = ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds;
            _processTryToRefreshAggregateIntervalMilliseconds = ENodeConfiguration.Instance.Setting.ProcessTryToRefreshAggregateIntervalMilliseconds;
        }

        public void Process(ProcessingEvent processingMessage)
        {
            var aggregateRootId = processingMessage.Message.AggregateRootId;
            if (string.IsNullOrEmpty(aggregateRootId))
            {
                throw new ArgumentException("aggregateRootId of domain event stream cannot be null or empty, domainEventStreamId:" + processingMessage.Message.Id);
            }

            var mailbox = _mailboxDict.GetOrAdd(aggregateRootId, x => new ProcessingEventMailBox(processingMessage.Message.AggregateRootTypeName, processingMessage.Message.AggregateRootId, y => DispatchProcessingMessageAsync(y, 0), _jsonSerializer, _logger));
            var mailboxTryUsingCount = 0L;
            while (!mailbox.TryUsing())
            {
                Thread.Sleep(1);
                mailboxTryUsingCount++;
                if (mailboxTryUsingCount % 10000 == 0)
                {
                    _logger.WarnFormat("Event mailbox try using count: {0}, aggregateRootId: {1}, aggregateRootTypeName: {2}", mailboxTryUsingCount, mailbox.AggregateRootId, mailbox.AggregateRootTypeName);
                }
            }
            if (mailbox.IsRemoved)
            {
                mailbox = _mailboxDict.GetOrAdd(aggregateRootId, x => new ProcessingEventMailBox(processingMessage.Message.AggregateRootTypeName, processingMessage.Message.AggregateRootId, y => DispatchProcessingMessageAsync(y, 0), _jsonSerializer, _logger));
            }
            ProcessingEventMailBox.EnqueueMessageResult enqueueResult = mailbox.EnqueueMessage(processingMessage);
            if (enqueueResult == ProcessingEventMailBox.EnqueueMessageResult.Ignored)
            {
                processingMessage.ProcessContext.NotifyEventProcessed();
            }
            else if (enqueueResult == ProcessingEventMailBox.EnqueueMessageResult.AddToWaitingList)
            {
                AddToRefreshAggregateMailBoxToDict(mailbox);
            }
            mailbox.ExitUsing();
        }
        public void Start()
        {
            _scheduleService.StartTask(_scanInactiveMailBoxTaskName, CleanInactiveMailbox, _scanExpiredAggregateIntervalMilliseconds, _scanExpiredAggregateIntervalMilliseconds);
            _scheduleService.StartTask(_processTryToRefreshAggregateTaskName, ProcessToRefreshAggregateRootMailBoxs, _processTryToRefreshAggregateIntervalMilliseconds, _processTryToRefreshAggregateIntervalMilliseconds);
        }
        public void Stop()
        {
            _scheduleService.StopTask(_scanInactiveMailBoxTaskName);
        }

        private void AddToRefreshAggregateMailBoxToDict(ProcessingEventMailBox mailbox)
        {
            if (_toRefreshAggregateRootMailBoxDict.TryAdd(mailbox.AggregateRootId, mailbox))
            {
                _logger.InfoFormat("Added toRefreshPublishedVersion aggregate mailbox, aggregateRootTypeName: {0}, aggregateRootId: {1}", mailbox.AggregateRootTypeName, mailbox.AggregateRootId);
                TryToRefreshAggregateMailBoxNextExpectingEventVersion(mailbox);
            }
        }
        private void TryToRefreshAggregateMailBoxNextExpectingEventVersion(ProcessingEventMailBox processingEventMailBox)
        {
            if (_refreshingAggregateRootDict.TryAdd(processingEventMailBox.AggregateRootId, true))
            {
                GetAggregateRootLatestPublishedEventVersion(processingEventMailBox, 0);
            }
        }
        private void ProcessToRefreshAggregateRootMailBoxs()
        {
            var entryList = _toRefreshAggregateRootMailBoxDict.ToArray();
            if (entryList.Length == 0)
            {
                return;
            }
            var remainingMailboxList = new List<ProcessingEventMailBox>();
            var recoveredMailboxList = new List<ProcessingEventMailBox>();
            foreach (var entry in entryList)
            {
                var aggregateRootMailBox = entry.Value;
                if (aggregateRootMailBox.WaitingMessageCount > 0)
                {
                    remainingMailboxList.Add(aggregateRootMailBox);
                }
                else
                {
                    recoveredMailboxList.Add(aggregateRootMailBox);
                }
            }
            foreach (var mailbox in remainingMailboxList)
            {
                TryToRefreshAggregateMailBoxNextExpectingEventVersion(mailbox);
            }
            foreach (var mailbox in recoveredMailboxList)
            {
                if (_toRefreshAggregateRootMailBoxDict.TryRemove(mailbox.AggregateRootId, out ProcessingEventMailBox removed))
                {
                    _logger.InfoFormat("Removed healthy aggregate mailbox, aggregateRootTypeName: {0}, aggregateRootId: {1}", removed.AggregateRootTypeName, removed.AggregateRootId);
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
        private void GetAggregateRootLatestPublishedEventVersion(ProcessingEventMailBox processingEventMailBox, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("GetAggregateRootLatestPublishedEventVersion",
            () => _publishedVersionStore.GetPublishedVersionAsync(Name, processingEventMailBox.AggregateRootTypeName, processingEventMailBox.AggregateRootId),
            currentRetryTimes => GetAggregateRootLatestPublishedEventVersion(processingEventMailBox, currentRetryTimes),
            result =>
            {
                processingEventMailBox.SetNextExpectingEventVersion(result + 1);
                _refreshingAggregateRootDict.TryRemove(processingEventMailBox.AggregateRootId, out bool removed);
            },
            () => string.Format("_publishedVersionStore.GetPublishedVersionAsync has unknown exception, aggregateRootTypeName: {0}, aggregateRootId: {1}", processingEventMailBox.AggregateRootTypeName, processingEventMailBox.AggregateRootId),
            null,
            retryTimes, true);
        }
        private void UpdatePublishedVersionAsync(ProcessingEvent processingMessage, int retryTimes)
        {
            var message = processingMessage.Message;
            _ioHelper.TryAsyncActionRecursivelyWithoutResult("UpdatePublishedVersionAsync",
            () => _publishedVersionStore.UpdatePublishedVersionAsync(Name, message.AggregateRootTypeName, message.AggregateRootId, message.Version),
            currentRetryTimes => UpdatePublishedVersionAsync(processingMessage, currentRetryTimes),
            () =>
            {
                _logger.InfoFormat("AggregateRoot publishedVersion updated, aggregateRootTypeName: {0}, aggregateRootId: {1}, publishedVersion: {2}", message.AggregateRootTypeName, message.AggregateRootId, message.Version);
                processingMessage.Complete();
            },
            () => string.Format("DomainEventStreamMessage [messageId:{0}, messageType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}]", message.Id, message.GetType().Name, message.AggregateRootId, message.Version),
            null,
            retryTimes, true);
        }
        private void CleanInactiveMailbox()
        {
            var inactiveList = new List<KeyValuePair<string, ProcessingEventMailBox>>();
            foreach (var pair in _mailboxDict)
            {
                if (IsMailBoxAllowRemove(pair.Value))
                {
                    inactiveList.Add(pair);
                }
            }
            foreach (var pair in inactiveList)
            {
                var mailbox = pair.Value;
                if (mailbox.TryUsing())
                {
                    if (IsMailBoxAllowRemove(mailbox))
                    {
                        if (_mailboxDict.TryRemove(pair.Key, out ProcessingEventMailBox removed))
                        {
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
