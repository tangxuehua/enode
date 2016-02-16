using System.Collections.Concurrent;
using System.Threading;
using ECommon.Utilities;

namespace ENode.Infrastructure
{
    public class ProcessingMessageMailbox<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        private ConcurrentDictionary<int, X> _waitingMessageDict;
        private readonly ConcurrentQueue<X> _messageQueue;
        private readonly IProcessingMessageScheduler<X, Y, Z> _scheduler;
        private readonly IProcessingMessageHandler<X, Y, Z> _messageHandler;
        private int _isHandlingMessage;
        private readonly object _lockObj = new object();

        public ProcessingMessageMailbox(IProcessingMessageScheduler<X, Y, Z> scheduler, IProcessingMessageHandler<X, Y, Z> messageHandler)
        {
            _messageQueue = new ConcurrentQueue<X>();
            _scheduler = scheduler;
            _messageHandler = messageHandler;
        }

        public void EnqueueMessage(X processingMessage)
        {
            processingMessage.SetMailbox(this);
            _messageQueue.Enqueue(processingMessage);
        }
        public bool EnterHandlingMessage()
        {
            return Interlocked.CompareExchange(ref _isHandlingMessage, 1, 0) == 0;
        }
        public void ExitHandlingMessage()
        {
            Interlocked.Exchange(ref _isHandlingMessage, 0);
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

            ExitHandlingMessage();
            RegisterForExecution();
        }
        public void CompleteMessage(X processingMessage)
        {
            if (!TryExecuteWaitingMessage(processingMessage))
            {
                ExitHandlingMessage();
                RegisterForExecution();
            }
        }
        public void Run()
        {
            X processingMessage = null;
            try
            {
                if (_messageQueue.TryDequeue(out processingMessage))
                {
                    _messageHandler.HandleAsync(processingMessage);
                }
            }
            finally
            {
                if (processingMessage == null)
                {
                    ExitHandlingMessage();
                    if (!_messageQueue.IsEmpty)
                    {
                        RegisterForExecution();
                    }
                }
            }
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
        private void RegisterForExecution()
        {
            _scheduler.ScheduleMailbox(this);
        }
    }
}
