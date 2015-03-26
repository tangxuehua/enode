using System.Collections.Concurrent;
using System.Threading;
using ECommon.Utilities;

namespace ENode.Infrastructure
{
    public class ProcessingMessageMailbox<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        private readonly ConcurrentDictionary<int, X> _waitingToRetryMessageDict;
        private readonly ConcurrentQueue<X> _messageQueue;
        private readonly IProcessingMessageScheduler<X, Y, Z> _mailboxScheduler;
        private readonly IProcessingMessageHandler<X, Y, Z> _messageHandler;
        private int _isHandlingMessage;

        public ProcessingMessageMailbox(IProcessingMessageScheduler<X, Y, Z> scheduler, IProcessingMessageHandler<X, Y, Z> messageHandler)
        {
            _waitingToRetryMessageDict = new ConcurrentDictionary<int, X>();
            _messageQueue = new ConcurrentQueue<X>();
            _mailboxScheduler = scheduler;
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
        public void AddWaitingForRetryMessage(X processingMessage)
        {
            var sequenceMessage = processingMessage.Message as ISequenceMessage;
            Ensure.NotNull(sequenceMessage, "processingMessage");
            _waitingToRetryMessageDict.TryAdd(sequenceMessage.Version, processingMessage);

            ExitHandlingMessage();
            RegisterForExecution();
        }
        public void CompleteMessage(X processingMessage)
        {
            if (!TryExecuteNextMessage(processingMessage))
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

        private bool TryExecuteNextMessage(X currentCompletedMessage)
        {
            var sequenceMessage = currentCompletedMessage.Message as ISequenceMessage;
            if (sequenceMessage == null) return false;

            X nextMessage;
            if (_waitingToRetryMessageDict.TryRemove(sequenceMessage.Version + 1, out nextMessage))
            {
                _mailboxScheduler.ScheduleMessage(nextMessage);
                return true;
            }
            return false;
        }
        private void RegisterForExecution()
        {
            _mailboxScheduler.ScheduleMailbox(this);
        }
    }
}
