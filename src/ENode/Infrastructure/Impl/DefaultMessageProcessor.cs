using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;

namespace ENode.Infrastructure
{
    public class DefaultMessageProcessor<X, Y> : IMessageProcessor<X, Y>
        where X : class, IProcessingMessage<X, Y>
        where Y : IMessage
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ProcessingMessageMailbox<X, Y>> _mailboxDict;
        private readonly IProcessingMessageScheduler<X, Y> _processingMessageScheduler;
        private readonly IProcessingMessageHandler<X, Y> _processingMessageHandler;
        private readonly IScheduleService _scheduleService;
        private readonly int _timeoutSeconds;
        private readonly string _taskName;

        public DefaultMessageProcessor(IProcessingMessageScheduler<X, Y> processingMessageScheduler, IProcessingMessageHandler<X, Y> processingMessageHandler, ILoggerFactory loggerFactory)
        {
            _mailboxDict = new ConcurrentDictionary<string, ProcessingMessageMailbox<X, Y>>();
            _processingMessageScheduler = processingMessageScheduler;
            _processingMessageHandler = processingMessageHandler;
            _logger = loggerFactory.Create(GetType().FullName);
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _timeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            _taskName = "CleanInactiveAggregates_" + DateTime.Now.Ticks + new Random().Next(10000);
        }

        public virtual string MessageName
        {
            get { return "message"; }
        }
        public void Process(X processingMessage)
        {
            var routingKey = processingMessage.Message.GetRoutingKey();
            if (!string.IsNullOrWhiteSpace(routingKey))
            {
                var mailbox = _mailboxDict.GetOrAdd(routingKey, x =>
                {
                    return new ProcessingMessageMailbox<X, Y>(routingKey, _processingMessageScheduler, _processingMessageHandler, _logger);
                });
                mailbox.EnqueueMessage(processingMessage);
            }
            else
            {
                _processingMessageScheduler.ScheduleMessage(processingMessage);
            }
        }
        public void Start()
        {
            _scheduleService.StartTask(_taskName, CleanInactiveMailbox, ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds, ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds);
        }
        public void Stop()
        {
            _scheduleService.StopTask(_taskName);
        }

        private void CleanInactiveMailbox()
        {
            var inactiveList = new List<KeyValuePair<string, ProcessingMessageMailbox<X, Y>>>();
            foreach (var pair in _mailboxDict)
            {
                if (pair.Value.IsInactive(_timeoutSeconds) && !pair.Value.IsRunning)
                {
                    inactiveList.Add(pair);
                }
            }
            foreach (var pair in inactiveList)
            {
                ProcessingMessageMailbox<X, Y> removed;
                if (_mailboxDict.TryRemove(pair.Key, out removed))
                {
                    _logger.InfoFormat("Removed inactive {0} mailbox, aggregateRootId: {1}", MessageName, pair.Key);
                }
            }
        }
    }
}
