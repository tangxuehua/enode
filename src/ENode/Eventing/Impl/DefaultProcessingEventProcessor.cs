using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.Infrastructure;
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
        private readonly ILogger _logger;
        private readonly IScheduleService _scheduleService;
        private readonly int _timeoutSeconds;
        private readonly string _taskName;
        private readonly string _processorName;
        private readonly int _scanExpiredAggregateIntervalMilliseconds;

        public DefaultProcessingEventProcessor(IPublishedVersionStore publishedVersionStore, IMessageDispatcher dispatcher, IOHelper ioHelper, ILoggerFactory loggerFactory, IScheduleService scheduleService)
        {
            _mailboxDict = new ConcurrentDictionary<string, ProcessingEventMailBox>();
            _publishedVersionStore = publishedVersionStore;
            _dispatcher = dispatcher;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
            _scheduleService = scheduleService;
            _taskName = "CleanInactiveProcessingEventMailBoxes_" + DateTime.Now.Ticks + new Random().Next(10000);
            _timeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            _processorName = ENodeConfiguration.Instance.Setting.DomainEventProcessorName;
            _scanExpiredAggregateIntervalMilliseconds = ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds;
        }

        public void Process(ProcessingEvent processingMessage)
        {
            var aggregateRootId = processingMessage.Message.AggregateRootId;
            if (string.IsNullOrEmpty(aggregateRootId))
            {
                throw new ArgumentException("aggregateRootId of domain event stream cannot be null or empty, domainEventStreamId:" + processingMessage.Message.Id);
            }

            lock (_lockObj)
            {
                var mailbox = _mailboxDict.GetOrAdd(aggregateRootId, x =>
                {
                    var latestHandledEventVersion = GetAggregateRootLatestHandledEventVersion(processingMessage.Message.AggregateRootTypeName, aggregateRootId);
                    return new ProcessingEventMailBox(aggregateRootId, latestHandledEventVersion, y => DispatchProcessingMessageAsync(y, 0), _logger);
                });
                mailbox.EnqueueMessage(processingMessage);
            }
        }
        public void Start()
        {
            _scheduleService.StartTask(_taskName, CleanInactiveMailbox, _scanExpiredAggregateIntervalMilliseconds, _scanExpiredAggregateIntervalMilliseconds);
        }
        public void Stop()
        {
            _scheduleService.StopTask(_taskName);
        }

        private void DispatchProcessingMessageAsync(ProcessingEvent processingMessage, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("DispatchProcessingMessageAsync",
            () => _dispatcher.DispatchMessagesAsync(processingMessage.Message.Events),
            currentRetryTimes => DispatchProcessingMessageAsync(processingMessage, currentRetryTimes),
            result =>
            {
                UpdatePublishedVersionAsync(processingMessage, 0);
            },
            () => string.Format("sequence message [messageId:{0}, messageType:{1}, aggregateRootId:{2}, aggregateRootVersion:{3}]", processingMessage.Message.Id, processingMessage.Message.GetType().Name, processingMessage.Message.AggregateRootId, processingMessage.Message.Version),
            null,
            retryTimes, true);
        }
        private int GetAggregateRootLatestHandledEventVersion(string aggregateRootType, string aggregateRootId)
        {
            try
            {
                var task = _publishedVersionStore.GetPublishedVersionAsync(_processorName, aggregateRootType, aggregateRootId);
                task.Wait();
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
                else if (task.Result.Status == AsyncTaskStatus.Success)
                {
                    return task.Result.Data;
                }
                else
                {
                    throw new Exception("_publishedVersionStore.GetPublishedVersionAsync has unknown exception, errorMessage: " + task.Result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("_publishedVersionStore.GetPublishedVersionAsync has unknown exception.", ex);
            }
        }
        private void UpdatePublishedVersionAsync(ProcessingEvent processingMessage, int retryTimes)
        {
            var message = processingMessage.Message;
            _ioHelper.TryAsyncActionRecursively("UpdatePublishedVersionAsync",
            () => _publishedVersionStore.UpdatePublishedVersionAsync(_processorName, message.AggregateRootTypeName, message.AggregateRootId, message.Version),
            currentRetryTimes => UpdatePublishedVersionAsync(processingMessage, currentRetryTimes),
            result =>
            {
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
                if (pair.Value.IsInactive(_timeoutSeconds) && !pair.Value.IsRunning)
                {
                    inactiveList.Add(pair);
                }
            }
            foreach (var pair in inactiveList)
            {
                lock (_lockObj)
                {
                    if (pair.Value.IsInactive(_timeoutSeconds) && !pair.Value.IsRunning && pair.Value.TotalUnHandledMessageCount == 0)
                    {
                        if (_mailboxDict.TryRemove(pair.Key, out ProcessingEventMailBox removed))
                        {
                            _logger.InfoFormat("Removed inactive domain event stream mailbox, aggregateRootId: {0}", pair.Key);
                        }
                    }
                }
            }
        }
    }
}
