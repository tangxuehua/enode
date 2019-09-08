using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class ProcessingEventMailBox
    {
        #region Private Variables 

        private readonly object _lockObj = new object();
        private readonly ConcurrentQueue<ProcessingEvent> _messageQueue;
        private readonly ConcurrentDictionary<int, ProcessingEvent> _waitingMessageDict = new ConcurrentDictionary<int, ProcessingEvent>();
        private readonly Action<ProcessingEvent> _handleMessageAction;
        private readonly ILogger _logger;

        #endregion

        public ProcessingEventMailBox(string aggregateRootId, int latestHandledEventVersion, Action<ProcessingEvent> handleMessageAction, ILogger logger)
        {
            _messageQueue = new ConcurrentQueue<ProcessingEvent>();
            _handleMessageAction = handleMessageAction;
            _logger = logger;
            AggregateRootId = aggregateRootId;
            LatestHandledEventVersion = latestHandledEventVersion;
            LastActiveTime = DateTime.Now;
        }

        public string AggregateRootId { get; private set; }
        public bool IsRunning { get; private set; }
        public int LatestHandledEventVersion { get; private set; }
        public long TotalUnHandledMessageCount
        {
            get
            {
                return _messageQueue.Count;
            }
        }
        public long WaitingMessageCount
        {
            get { return _waitingMessageDict.Count; }
        }
        public DateTime LastActiveTime { get; private set; }

        public void EnqueueMessage(ProcessingEvent message)
        {
            lock (_lockObj)
            {
                var eventStream = message.Message;
                if (eventStream.Version == LatestHandledEventVersion + 1)
                {
                    message.MailBox = this;
                    _messageQueue.Enqueue(message);
                    _logger.DebugFormat("{0} enqueued new message, aggregateRootType: {1}, aggregateRootId: {2}, commandId: {3}, eventVersion: {4}, eventStreamId: {5}, eventTypes: {6}, eventIds: {7}",
                        GetType().Name,
                        eventStream.AggregateRootTypeName,
                        eventStream.AggregateRootId,
                        eventStream.CommandId,
                        eventStream.Version,
                        eventStream.Id,
                        string.Join("|", eventStream.Events.Select(x => x.GetType().Name)),
                        string.Join("|", eventStream.Events.Select(x => x.Id))
                    );
                    LatestHandledEventVersion = eventStream.Version;

                    var nextVersion = eventStream.Version + 1;
                    while (_waitingMessageDict.TryRemove(nextVersion, out ProcessingEvent nextMessage))
                    {
                        var nextEventStream = nextMessage.Message;
                        nextMessage.MailBox = this;
                        _messageQueue.Enqueue(nextMessage);
                        LatestHandledEventVersion = nextEventStream.Version;
                        _logger.DebugFormat("{0} enqueued new message, aggregateRootType: {1}, aggregateRootId: {2}, commandId: {3}, eventVersion: {4}, eventStreamId: {5}, eventTypes: {6}, eventIds: {7}",
                            GetType().Name,
                            eventStream.AggregateRootTypeName,
                            nextEventStream.AggregateRootId,
                            nextEventStream.CommandId,
                            nextEventStream.Version,
                            nextEventStream.Id,
                            string.Join("|", eventStream.Events.Select(x => x.GetType().Name)),
                            string.Join("|", nextEventStream.Events.Select(x => x.Id))
                        );
                        nextVersion++;
                    }

                    LastActiveTime = DateTime.Now;
                    TryRun();
                }
                else if (eventStream.Version > LatestHandledEventVersion + 1)
                {
                    _waitingMessageDict.TryAdd(eventStream.Version, message);
                }
            }
        }
        public void TryRun()
        {
            lock (_lockObj)
            {
                if (IsRunning)
                {
                    return;
                }
                SetAsRunning();
                _logger.DebugFormat("{0} start run, aggregateRootId: {1}", GetType().Name, AggregateRootId);
                Task.Factory.StartNew(ProcessMessage);
            }
        }
        public void CompleteRun()
        {
            LastActiveTime = DateTime.Now;
            _logger.DebugFormat("{0} complete run, aggregateRootId: {1}", GetType().Name, AggregateRootId);
            SetAsNotRunning();
            if (TotalUnHandledMessageCount > 0)
            {
                TryRun();
            }
        }
        public bool IsInactive(int timeoutSeconds)
        {
            return (DateTime.Now - LastActiveTime).TotalSeconds >= timeoutSeconds;
        }

        private void ProcessMessage()
        {
            if (_messageQueue.TryDequeue(out ProcessingEvent message))
            {
                LastActiveTime = DateTime.Now;
                try
                {
                    _handleMessageAction(message);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{0} run has unknown exception, aggregateRootId: {1}", GetType().Name, AggregateRootId), ex);
                    Thread.Sleep(1);
                    CompleteRun();
                }
            }
            else
            {
                CompleteRun();
            }
        }
        private void SetAsRunning()
        {
            IsRunning = true;
        }
        private void SetAsNotRunning()
        {
            IsRunning = false;
        }
    }
}