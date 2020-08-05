using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class EventCommittingContextMailBox
    {
        #region Private Variables 

        private readonly object _lockObj = new object();
        private readonly object _processMessageLockObj = new object();
        private readonly ConcurrentQueue<EventCommittingContext> _messageQueue;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Byte>> _aggregateDictDict;
        private readonly Action<IList<EventCommittingContext>> _handleMessageAction;
        private readonly int _batchSize;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;

        #endregion

        public EventCommittingContextMailBox(int number, int batchSize, Action<IList<EventCommittingContext>> handleMessageAction, IJsonSerializer jsonSerializer, ILogger logger)
        {
            _messageQueue = new ConcurrentQueue<EventCommittingContext>();
            _aggregateDictDict = new ConcurrentDictionary<string, ConcurrentDictionary<string, Byte>>();
            _handleMessageAction = handleMessageAction;
            _jsonSerializer = jsonSerializer;
            _logger = logger;
            _batchSize = batchSize;
            Number = number;
            LastActiveTime = DateTime.Now;
        }

        public DateTime LastActiveTime { get; private set; }
        public int Number { get; private set; }
        public bool IsRunning { get; private set; }
        public long TotalUnHandledMessageCount
        {
            get
            {
                return _messageQueue.Count;
            }
        }

        public void EnqueueMessage(EventCommittingContext message)
        {
            lock (_lockObj)
            {
                var eventDict = _aggregateDictDict.GetOrAdd(message.EventStream.AggregateRootId, x => new ConcurrentDictionary<string, byte>());
                if (eventDict.TryAdd(message.EventStream.Id, 1))
                {
                    message.MailBox = this;
                    _messageQueue.Enqueue(message);
                    _logger.InfoFormat("{0} enqueued new message, mailboxNumber: {1}, message: {2}", GetType().Name, Number, _jsonSerializer.Serialize(message.EventStream));
                    LastActiveTime = DateTime.Now;
                    TryRun();
                }
                else
                {
                    throw new DuplicateEventStreamException(message.EventStream);
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
                _logger.InfoFormat("{0} start run, mailboxNumber: {1}", GetType().Name, Number);
                Task.Factory.StartNew(ProcessMessages);
            }
        }
        public void CompleteRun()
        {
            LastActiveTime = DateTime.Now;
            _logger.InfoFormat("{0} complete run, mailboxNumber: {1}", GetType().Name, Number);
            SetAsNotRunning();
            if (TotalUnHandledMessageCount > 0)
            {
                TryRun();
            }
        }
        public void RemoveAggregateAllEventCommittingContexts(string aggregateRootId)
        {
            _aggregateDictDict.TryRemove(aggregateRootId, out ConcurrentDictionary<string, Byte> removed);
        }
        public bool IsInactive(int timeoutSeconds)
        {
            return (DateTime.Now - LastActiveTime).TotalSeconds >= timeoutSeconds;
        }

        private void ProcessMessages()
        {
            lock (_processMessageLockObj)
            {
                LastActiveTime = DateTime.Now;
                var messageList = new List<EventCommittingContext>();

                while (messageList.Count < _batchSize)
                {
                    if (_messageQueue.TryDequeue(out EventCommittingContext message))
                    {
                        if (_aggregateDictDict.TryGetValue(message.EventStream.AggregateRootId, out ConcurrentDictionary<string, Byte> eventDict)
                            && eventDict.TryRemove(message.EventStream.Id, out byte removed))
                        {
                            messageList.Add(message);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (messageList.Count == 0)
                {
                    CompleteRun();
                    return;
                }

                try
                {
                    _handleMessageAction(messageList);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{0} run has unknown exception, mailboxNumber: {1}", GetType().Name, Number), ex);
                    Thread.Sleep(1);
                    CompleteRun();
                }
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