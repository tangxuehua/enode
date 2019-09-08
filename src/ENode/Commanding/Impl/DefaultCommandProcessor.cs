using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandProcessor : ICommandProcessor
    {
        private readonly object _lockObj = new object();
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ProcessingCommandMailbox> _mailboxDict;
        private readonly IProcessingCommandHandler _handler;
        private readonly IScheduleService _scheduleService;
        private readonly int _timeoutSeconds;
        private readonly string _taskName;

        public DefaultCommandProcessor(IScheduleService scheduleService, IProcessingCommandHandler handler, ILoggerFactory loggerFactory)
        {
            _scheduleService = scheduleService;
            _mailboxDict = new ConcurrentDictionary<string, ProcessingCommandMailbox>();
            _handler = handler;
            _logger = loggerFactory.Create(GetType().FullName);
            _timeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            _taskName = "CleanInactiveProcessingCommandMailBoxes_" + DateTime.Now.Ticks + new Random().Next(10000);
        }

        public void Process(ProcessingCommand processingCommand)
        {
            var aggregateRootId = processingCommand.Message.AggregateRootId;
            if (string.IsNullOrEmpty(aggregateRootId))
            {
                throw new ArgumentException("aggregateRootId of command cannot be null or empty, commandId:" + processingCommand.Message.Id);
            }

            lock (_lockObj)
            {
                var mailbox = _mailboxDict.GetOrAdd(aggregateRootId, x =>
                {
                    return new ProcessingCommandMailbox(x, _handler, _logger);
                });
                mailbox.EnqueueMessage(processingCommand);
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
            var inactiveList = new List<KeyValuePair<string, ProcessingCommandMailbox>>();

            foreach (var pair in _mailboxDict)
            {
                if (pair.Value.IsInactive(_timeoutSeconds) && !pair.Value.IsRunning && pair.Value.TotalUnHandledMessageCount == 0)
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
                        if (_mailboxDict.TryRemove(pair.Key, out ProcessingCommandMailbox removed))
                        {
                            _logger.InfoFormat("Removed inactive command mailbox, aggregateRootId: {0}", pair.Key);
                        }
                    }
                }
            }
        }
    }
}
