using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class ProcessingCommandMailbox
    {
        #region Private Variables 

        private readonly object _lockObj = new object();
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly ConcurrentDictionary<long, ProcessingCommand> _messageDict;
        private readonly IProcessingCommandHandler _messageHandler;
        private readonly int _batchSize;
        private readonly ILogger _logger;

        #endregion

        public ProcessingCommandMailbox(string aggregateRootId, IProcessingCommandHandler messageHandler, ILogger logger)
        {
            _messageDict = new ConcurrentDictionary<long, ProcessingCommand>();
            _messageHandler = messageHandler;
            _logger = logger;
            _batchSize = ENodeConfiguration.Instance.Setting.CommandMailBoxProcessBatchSize;
            AggregateRootId = aggregateRootId;
            LastActiveTime = DateTime.Now;
        }

        public string AggregateRootId { get; private set; }
        public DateTime LastActiveTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPauseRequested { get; private set; }
        public bool IsPaused { get; private set; }
        public long NextSequence { get; private set; }
        public long ConsumingSequence { get; private set; }
        public long TotalUnHandledMessageCount
        {
            get
            {
                return NextSequence - ConsumingSequence;
            }
        }
        public long MaxMessageSequence
        {
            get
            {
                return NextSequence - 1;
            }
        }

        public void EnqueueMessage(ProcessingCommand message)
        {
            lock (_lockObj)
            {
                message.Sequence = NextSequence;
                message.MailBox = this;
                if (_messageDict.TryAdd(message.Sequence, message))
                {
                    NextSequence++;
                    _logger.DebugFormat("{0} enqueued new message, aggregateRootId: {1}, messageId: {2}, messageSequence: {3}", GetType().Name, AggregateRootId, message.Message.Id, message.Sequence);
                    LastActiveTime = DateTime.Now;
                    TryRun();
                }
            }
        }
        public void TryRun()
        {
            lock (_lockObj)
            {
                if (IsRunning || IsPauseRequested || IsPaused)
                {
                    return;
                }
                SetAsRunning();
                _logger.DebugFormat("{0} start run, aggregateRootId: {1}, consumingSequence: {2}", GetType().Name, AggregateRootId, ConsumingSequence);
                Task.Factory.StartNew(ProcessMessages);
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
        public void Pause()
        {
            IsPauseRequested = true;
            _logger.DebugFormat("{0} pause requested, aggregateRootId: {1}", GetType().Name, AggregateRootId);
            var count = 0L;
            while (IsRunning)
            {
                Thread.Sleep(10);
                count++;
                if (count % 100 == 0)
                {
                    _logger.DebugFormat("{0} pause requested, but wait for too long to stop the current mailbox, aggregateRootId: {1}, waitCount: {2}", GetType().Name, AggregateRootId, count);
                }
            }
            LastActiveTime = DateTime.Now;
            IsPaused = true;
        }
        public void Resume()
        {
            IsPauseRequested = false;
            IsPaused = false;
            LastActiveTime = DateTime.Now;
            _logger.DebugFormat("{0} resume requested, aggregateRootId: {1}, consumingSequence: {2}", GetType().Name, AggregateRootId, ConsumingSequence);
        }
        public void ResetConsumingSequence(long consumingSequence)
        {
            ConsumingSequence = consumingSequence;
            LastActiveTime = DateTime.Now;
            _logger.DebugFormat("{0} reset consumingSequence, aggregateRootId: {1}, consumingSequence: {2}", GetType().Name, AggregateRootId, consumingSequence);
        }
        public void Clear()
        {
            _messageDict.Clear();
            NextSequence = 0;
            ConsumingSequence = 0;
            LastActiveTime = DateTime.Now;
        }
        public async Task CompleteMessage(ProcessingCommand message, CommandResult result)
        {
            try
            {
                if (_messageDict.TryRemove(message.Sequence, out ProcessingCommand removed))
                {
                    LastActiveTime = DateTime.Now;
                    await message.CompleteAsync(result).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("{0} complete message with result failed, aggregateRootId: {1}, messageId: {2}, messageSequence: {3}, result: {4}", GetType().Name, AggregateRootId, message.Message.Id, message.Sequence, result), ex);
            }
        }
        public bool IsInactive(int timeoutSeconds)
        {
            return (DateTime.Now - LastActiveTime).TotalSeconds >= timeoutSeconds;
        }

        private async Task ProcessMessages()
        {
            using (await _asyncLock.LockAsync().ConfigureAwait(false))
            {
                LastActiveTime = DateTime.Now;
                try
                {
                    var scannedCount = 0;
                    while (TotalUnHandledMessageCount > 0 && scannedCount < _batchSize && !IsPauseRequested)
                    {
                        var message = GetMessage(ConsumingSequence);
                        if (message != null)
                        {
                            await _messageHandler.HandleAsync(message).ConfigureAwait(false);
                        }
                        scannedCount++;
                        ConsumingSequence++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{0} run has unknown exception, aggregateRootId: {1}", GetType().Name, AggregateRootId), ex);
                    Thread.Sleep(1);
                }
                finally
                {
                    CompleteRun();
                }
            }
        }
        private ProcessingCommand GetMessage(long sequence)
        {
            if (_messageDict.TryGetValue(sequence, out ProcessingCommand message))
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
