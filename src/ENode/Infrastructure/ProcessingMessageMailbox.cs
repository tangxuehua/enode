using System;
using System.Collections.Concurrent;
using System.Threading;
using ECommon.Logging;
using ECommon.Utilities;

namespace ENode.Infrastructure
{
    public class ProcessingMessageMailbox<X, Y>
        where X : class, IProcessingMessage<X, Y>
        where Y : IMessage
    {
        #region Private Variables 

        private readonly string _routingKey;
        private readonly ILogger _logger;
        private ConcurrentDictionary<int, X> _waitingMessageDict;
        private readonly ConcurrentQueue<X> _messageQueue;
        private readonly IProcessingMessageScheduler<X, Y> _scheduler;
        private readonly IProcessingMessageHandler<X, Y> _messageHandler;
        private int _isRunning;
        private readonly object _lockObj = new object();
        private DateTime _lastActiveTime;

        #endregion

        public string RoutingKey
        {
            get { return _routingKey; }
        }
        public DateTime LastActiveTime
        {
            get { return _lastActiveTime; }
        }
        public bool IsRunning
        {
            get { return _isRunning == 1; }
        }

        public ProcessingMessageMailbox(string routingKey, IProcessingMessageScheduler<X, Y> scheduler, IProcessingMessageHandler<X, Y> messageHandler, ILogger logger)
        {
            _routingKey = routingKey;
            _messageQueue = new ConcurrentQueue<X>();
            _scheduler = scheduler;
            _messageHandler = messageHandler;
            _logger = logger;
            _lastActiveTime = DateTime.Now;
        }

        public void EnqueueMessage(X processingMessage)
        {
            processingMessage.SetMailbox(this);
            _messageQueue.Enqueue(processingMessage);
            _lastActiveTime = DateTime.Now;
            TryRun();
        }
        public void AddWaitingMessage(X waitingMessage)
        {
            var sequenceMessage = waitingMessage.Message as ISequenceMessage;
            Ensure.NotNull(sequenceMessage, "sequenceMessage");

            if (_waitingMessageDict == null)
            {
                lock (_lockObj)
                {
                    if (_waitingMessageDict == null)
                    {
                        _waitingMessageDict = new ConcurrentDictionary<int, X>();
                    }
                }
            }

            _waitingMessageDict.TryAdd(sequenceMessage.Version, waitingMessage);
            _lastActiveTime = DateTime.Now;
            Exit();
            TryRun();
        }
        public void CompleteMessage(X processingMessage)
        {
            _lastActiveTime = DateTime.Now;
            if (!TryExecuteWaitingMessage(processingMessage))
            {
                Exit();
                TryRun();
            }
        }
        public void Run()
        {
            _lastActiveTime = DateTime.Now;
            X processingMessage = null;
            try
            {
                if (_messageQueue.TryDequeue(out processingMessage))
                {
                    _messageHandler.HandleAsync(processingMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Message mailbox run has unknown exception, routingKey: {0}, commandId: {1}", _routingKey, processingMessage != null ? processingMessage.Message.Id : string.Empty), ex);
                Thread.Sleep(1);
            }
            finally
            {
                if (processingMessage == null)
                {
                    Exit();
                    if (!_messageQueue.IsEmpty)
                    {
                        TryRun();
                    }
                }
            }
        }
        public bool IsInactive(int timeoutSeconds)
        {
            return (DateTime.Now - LastActiveTime).TotalSeconds >= timeoutSeconds;
        }

        private bool TryExecuteWaitingMessage(X currentCompletedMessage)
        {
            var sequenceMessage = currentCompletedMessage.Message as ISequenceMessage;
            if (sequenceMessage == null) return false;

            X nextMessage;
            if (_waitingMessageDict != null && _waitingMessageDict.TryRemove(sequenceMessage.Version + 1, out nextMessage))
            {
                _scheduler.ScheduleMessage(nextMessage);
                return true;
            }
            return false;
        }
        private void TryRun()
        {
            if (TryEnter())
            {
                _scheduler.ScheduleMailbox(this);
            }
        }
        private bool TryEnter()
        {
            return Interlocked.CompareExchange(ref _isRunning, 1, 0) == 0;
        }
        private void Exit()
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
    }
}
