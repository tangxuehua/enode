using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Extensions;
using ECommon.Logging;
using ENode.Commanding;

namespace ENode.Infrastructure
{
    public class DefaultMailBox<TMessage, TMessageProcessResult> : IMailBox<TMessage, TMessageProcessResult>
            where TMessage : class, IMailBoxMessage<TMessage, TMessageProcessResult>
    {
        #region Private Variables 

        private readonly object _lockObj = new object();
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly ConcurrentDictionary<long, TMessage> _messageDict;
        private readonly Dictionary<long, TMessageProcessResult> _requestToCompleteMessageDict;
        private readonly Func<TMessage, Task> _messageHandler;
        private readonly Func<IList<TMessage>, Task> _messageListHandler;
        private readonly bool _isBatchMessageProcess;
        private readonly int _batchSize;
        private long _nextSequence;

        #endregion

        protected readonly ILogger _logger;

        public string RoutingKey { get; private set; }
        public DateTime LastActiveTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPauseRequested { get; private set; }
        public bool IsPaused { get; private set; }
        public long ConsumingSequence { get; private set; }
        public long ConsumedSequence { get; private set; }
        public long MaxMessageSequence
        {
            get
            {
                return _nextSequence - 1;
            }
        }
        public long TotalUnConsumedMessageCount
        {
            get
            {
                return _nextSequence - 1 - ConsumedSequence;
            }
        }

        public DefaultMailBox(string routingKey, int batchSize, bool isBatchMessageProcess, Func<TMessage, Task> messageHandler, Func<IList<TMessage>, Task> messageListHandler, ILogger logger)
        {
            _messageDict = new ConcurrentDictionary<long, TMessage>();
            _requestToCompleteMessageDict = new Dictionary<long, TMessageProcessResult>();
            _batchSize = batchSize;
            RoutingKey = routingKey;
            _isBatchMessageProcess = isBatchMessageProcess;
            _messageHandler = messageHandler;
            _messageListHandler = messageListHandler;
            _logger = logger;
            ConsumedSequence = -1;
            LastActiveTime = DateTime.Now;
            if (isBatchMessageProcess && messageListHandler == null)
            {
                throw new ArgumentNullException("Parameter messageListHandler cannot be null");
            }
            else if (!isBatchMessageProcess && messageHandler == null)
            {
                throw new ArgumentNullException("Parameter messageHandler cannot be null");
            }
        }

        /// <summary>放入一个消息到MailBox，并自动尝试运行MailBox
        /// </summary>
        /// <param name="message"></param>
        public virtual void EnqueueMessage(TMessage message)
        {
            lock (_lockObj)
            {
                message.Sequence = _nextSequence;
                message.MailBox = this;
                if (_messageDict.TryAdd(message.Sequence, message))
                {
                    _nextSequence++;
                    _logger.DebugFormat("{0} enqueued new message, routingKey: {1}, messageSequence: {2}", GetType().Name, RoutingKey, message.Sequence);
                    LastActiveTime = DateTime.Now;
                    TryRun();
                }
            }
        }
        /// <summary>尝试运行一次MailBox，一次运行会处理一个消息或者一批消息，当前MailBox不能是运行中或者暂停中或者已暂停
        /// </summary>
        public virtual void TryRun()
        {
            lock (_lockObj)
            {
                if (IsRunning || IsPauseRequested || IsPaused)
                {
                    return;
                }
                SetAsRunning();
                _logger.DebugFormat("{0} start run, routingKey: {1}, consumingSequence: {2}", GetType().Name, RoutingKey, ConsumingSequence);
                Task.Factory.StartNew(ProcessMessages);
            }
        }
        /// <summary>请求完成MailBox的单次运行，如果MailBox中还有剩余消息，则继续尝试运行下一次
        /// </summary>
        public virtual void CompleteRun()
        {
            _logger.DebugFormat("{0} complete run, routingKey: {1}", GetType().Name, RoutingKey);
            SetAsNotRunning();
            if (HasNextMessage())
            {
                TryRun();
            }
        }
        /// <summary>暂停当前MailBox的运行，暂停成功可以确保当前MailBox不会处于运行状态，也就是不会在处理任何消息
        /// </summary>
        public virtual void Pause()
        {
            IsPauseRequested = true;
            _logger.DebugFormat("{0} pause requested, routingKey: {1}", GetType().Name, RoutingKey);
            var count = 0L;
            while (IsRunning)
            {
                Thread.Sleep(10);
                count++;
                if (count % 100 == 0)
                {
                    _logger.DebugFormat("{0} pause requested, but wait for too long to stop the current mailbox, routingKey: {1}, waitCount: {2}", GetType().Name, RoutingKey, count);
                }
            }
            IsPaused = true;
        }
        /// <summary>恢复当前MailBox的运行，恢复后，当前MailBox又可以进行运行，需要手动调用TryRun方法来运行
        /// </summary>
        public virtual void Resume()
        {
            IsPauseRequested = false;
            IsPaused = false;
            _logger.DebugFormat("{0} resume requested, routingKey: {1}, consumingSequence: {2}", GetType().Name, RoutingKey, ConsumingSequence);
        }
        public virtual void ResetConsumingSequence(long consumingSequence)
        {
            LastActiveTime = DateTime.Now;
            ConsumingSequence = consumingSequence;
            _requestToCompleteMessageDict.Clear();
            _logger.DebugFormat("{0} reset consumingSequence, routingKey: {1}, consumingSequence: {2}", GetType().Name, RoutingKey, consumingSequence);
        }
        public virtual void Clear()
        {
            _messageDict.Clear();
            _requestToCompleteMessageDict.Clear();
            _nextSequence = 0;
            ConsumingSequence = 0;
            ConsumedSequence = -1;
            LastActiveTime = DateTime.Now;
        }
        public virtual async Task CompleteMessage(TMessage message, TMessageProcessResult result)
        {
            using (await _asyncLock.LockAsync())
            {
                LastActiveTime = DateTime.Now;
                try
                {
                    if (message.Sequence == ConsumedSequence + 1)
                    {
                        _messageDict.Remove(message.Sequence);
                        await CompleteMessageWithResult(message, result);
                        ConsumedSequence = ProcessNextCompletedMessages(message.Sequence);
                    }
                    else if (message.Sequence > ConsumedSequence + 1)
                    {
                        _requestToCompleteMessageDict[message.Sequence] = result;
                    }
                    else if (message.Sequence < ConsumedSequence + 1)
                    {
                        _messageDict.Remove(message.Sequence);
                        await CompleteMessageWithResult(message, result);
                        _requestToCompleteMessageDict.Remove(message.Sequence);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("MailBox complete message with result failed, routingKey: {0}, message: {1}, result: {2}", RoutingKey, message, result), ex);
                }
            }
        }
        public virtual bool IsInactive(int timeoutSeconds)
        {
            return (DateTime.Now - LastActiveTime).TotalSeconds >= timeoutSeconds;
        }

        protected virtual Task CompleteMessageWithResult(TMessage message, TMessageProcessResult result)
        {
            return Task.CompletedTask;
        }
        protected virtual IList<TMessage> FilterMessages(IList<TMessage> messages)
        {
            return messages;
        }

        private async Task ProcessMessages()
        {
            LastActiveTime = DateTime.Now;
            try
            {
                if (_isBatchMessageProcess)
                {
                    var consumingSequence = ConsumingSequence;
                    var scannedSequenceSize = 0;
                    var messageList = new List<TMessage>();

                    while (HasNextMessage(consumingSequence) && scannedSequenceSize < _batchSize && !IsPauseRequested)
                    {
                        var message = GetMessage(consumingSequence);
                        if (message != null)
                        {
                            messageList.Add(message);
                        }
                        scannedSequenceSize++;
                        consumingSequence++;
                    }

                    var filterMessages = FilterMessages(messageList);
                    if (filterMessages != null && filterMessages.Count > 0)
                    {
                        await _messageListHandler(filterMessages);
                    }
                    ConsumingSequence = consumingSequence;

                    if (filterMessages == null || filterMessages.Count == 0)
                    {
                        CompleteRun();
                    }
                }
                else
                {
                    var scannedSequenceSize = 0;
                    while (HasNextMessage() && scannedSequenceSize < _batchSize && !IsPauseRequested)
                    {
                        var message = GetMessage(ConsumingSequence);
                        if (message != null)
                        {
                            await _messageHandler(message);
                        }
                        scannedSequenceSize++;
                        ConsumingSequence++;
                    }
                    CompleteRun();
                }  
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("MailBox run has unknown exception, mailboxType: {0}, routingKey: {1}", GetType().Name, RoutingKey), ex);
                Thread.Sleep(1);
            }
        }
        private long ProcessNextCompletedMessages(long baseSequence)
        {
            var returnSequence = baseSequence;
            var nextSequence = baseSequence + 1;
            while (_requestToCompleteMessageDict.ContainsKey(nextSequence))
            {
                if (_messageDict.TryRemove(nextSequence, out TMessage message))
                {
                    var result = _requestToCompleteMessageDict[nextSequence];
                    CompleteMessageWithResult(message, result);
                }
                _requestToCompleteMessageDict.Remove(nextSequence);
                returnSequence = nextSequence;
                nextSequence++;
            }
            return returnSequence;
        }
        private bool HasNextMessage(long? consumingSequence = null)
        {
            if (consumingSequence != null)
            {
                return consumingSequence.Value < _nextSequence;
            }
            return ConsumingSequence < _nextSequence;
        }
        private TMessage GetMessage(long sequence)
        {
            if (_messageDict.TryGetValue(sequence, out TMessage message))
            {
                return message;
            }
            return null;
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
