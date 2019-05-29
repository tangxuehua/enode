using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Extensions;
using ECommon.Logging;

namespace ENode.Infrastructure
{
    public class AggregateMessageMailBox<TMessage, TMessageProcessResult> : IAggregateMessageMailBox<TMessage, TMessageProcessResult>
            where TMessage : class, IAggregateMessageMailBoxMessage<TMessage, TMessageProcessResult>
    {
        #region Private Variables 

        private readonly object _lockObj = new object();
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly ConcurrentDictionary<long, TMessage> _messageDict;
        private readonly Dictionary<long, TMessageProcessResult> _requestToCompleteMessageDict;
        private readonly Func<TMessage, Task> _messageHandler;
        private readonly Func<IList<TMessage>, Task> _messageListHandler;
        private readonly ManualResetEvent _processingWaitHandle;
        private readonly bool _isBatchMessageProcess;
        private readonly int _batchSize;
        private long _nextSequence;
        private volatile int _isRunning;
        private volatile bool _isPauseRequested;
        private volatile bool _isPaused;

        #endregion

        protected readonly ILogger _logger;

        public string AggregateRootId { get; private set; }
        public DateTime LastActiveTime { get; private set; }
        public bool IsRunning
        {
            get { return _isRunning == 1; }
        }
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

        public AggregateMessageMailBox(string aggregateRootId, int batchSize, bool isBatchMessageProcess, Func<TMessage, Task> messageHandler, Func<IList<TMessage>, Task> messageListHandler, ILogger logger)
        {
            _messageDict = new ConcurrentDictionary<long, TMessage>();
            _requestToCompleteMessageDict = new Dictionary<long, TMessageProcessResult>();
            _processingWaitHandle = new ManualResetEvent(false);
            _batchSize = batchSize;
            AggregateRootId = aggregateRootId;
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

        public void EnqueueMessage(TMessage message)
        {
            lock (_lockObj)
            {
                message.Sequence = _nextSequence;
                message.MailBox = this;
                if (_messageDict.TryAdd(message.Sequence, message))
                {
                    _nextSequence++;
                    LastActiveTime = DateTime.Now;
                }
            }
            TryRun();
        }
        public void TryRun(bool exitFirst = false)
        {
            if (exitFirst)
            {
                Exit();
            }
            if (TryEnter())
            {
                Task.Factory.StartNew(async () => await Run());
            }
        }
        public async Task Run()
        {
            LastActiveTime = DateTime.Now;
            //如果当前已经被请求了暂停或者已经暂停了，则不应该在运行Run里的逻辑
            if (_isPauseRequested || _isPaused)
            {
                Exit();
                return;
            }
            TMessage message = null;
            try
            {
                //设置运行信号，表示当前正在运行Run方法中的逻辑
                _processingWaitHandle.Reset();
                var count = 0;
                IList<TMessage> messageList = null;
                while (ConsumingSequence < _nextSequence && count < _batchSize && !_isPauseRequested && !_isPaused)
                {
                    message = GetMessage(ConsumingSequence);
                    if (message != null)
                    {
                        if (_isBatchMessageProcess)
                        {
                            if (messageList == null)
                            {
                                messageList = new List<TMessage>();
                            }
                            messageList.Add(message);
                        }
                        else
                        {
                            await _messageHandler(message);
                        }
                    }
                    ConsumingSequence++;
                    count++;
                }

                if (_isBatchMessageProcess)
                {
                    if (messageList != null && messageList.Count > 0)
                    {
                        await _messageListHandler(messageList);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Aggregate mailbox run has unknown exception, aggregateRootId: {0}, consumingSequence: {1}, message: {2}", AggregateRootId, ConsumingSequence, message), ex);
                Thread.Sleep(1);
            }
            finally
            {
                //设置运行信号，表示当前Run方法中的逻辑运行完成
                _processingWaitHandle.Set();
                Exit();
                if (ConsumingSequence < _nextSequence)
                {
                    TryRun();
                }
            }
        }
        public void Pause()
        {
            //设置变量，表示当前正在请求暂停
            _isPauseRequested = true;
            var count = 0;
            //等待当前正在处理消息的线程退出
            while (!_processingWaitHandle.WaitOne(1000))
            {
                _logger.InfoFormat("Request to pause the aggregate mailbox, but the mailbox is currently processing message, aggregateRootId: {0}, waiteSseconds: {1}", AggregateRootId, count + 1);
                count++;
            }
            //设置变量，表示当前已经暂停成功
            _isPaused = true;
            LastActiveTime = DateTime.Now;
        }
        public void Resume()
        {
            _isPauseRequested = false;
            _isPaused = false;
            LastActiveTime = DateTime.Now;
            TryRun();
        }
        public void ResetConsumingSequence(long consumingSequence)
        {
            LastActiveTime = DateTime.Now;
            ConsumingSequence = consumingSequence;
            _requestToCompleteMessageDict.Clear();
        }
        public void Exit()
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
        public void Clear()
        {
            _messageDict.Clear();
            _requestToCompleteMessageDict.Clear();
            _processingWaitHandle.Reset();
            _nextSequence = 0;
            ConsumingSequence = 0;
            ConsumedSequence = -1;
            LastActiveTime = DateTime.Now;
        }
        public async Task CompleteMessage(TMessage message, TMessageProcessResult result)
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
                    _logger.Error(string.Format("Aggregate mailbox complete message with result failed, aggregateRootId: {0}, message: {1}, result: {2}", AggregateRootId, message, result), ex);
                }
            }
        }
        protected virtual Task CompleteMessageWithResult(TMessage message, TMessageProcessResult result)
        {
            return Task.CompletedTask;
        }
        public bool IsInactive(int timeoutSeconds)
        {
            return (DateTime.Now - LastActiveTime).TotalSeconds >= timeoutSeconds;
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
        private TMessage GetMessage(long sequence)
        {
            if (_messageDict.TryGetValue(sequence, out TMessage message))
            {
                return message;
            }
            return null;
        }
        private bool TryEnter()
        {
            if (_isPauseRequested || _isPaused)
            {
                return false;
            }
            return Interlocked.CompareExchange(ref _isRunning, 1, 0) == 0;
        }
    }
}
