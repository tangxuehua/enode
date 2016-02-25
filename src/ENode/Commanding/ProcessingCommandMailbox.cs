using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ENode.Commanding
{
    public class ProcessingCommandMailbox
    {
        private readonly string _aggregateRootId;
        private readonly ConcurrentQueue<ProcessingCommand> _messageQueue;
        private readonly IProcessingCommandScheduler _scheduler;
        private readonly IProcessingCommandHandler _messageHandler;
        private int _isHandlingMessage;
        private readonly object _lockObj = new object();

        public string AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }

        public ProcessingCommandMailbox(string aggregateRootId, IProcessingCommandScheduler scheduler, IProcessingCommandHandler messageHandler)
        {
            _messageQueue = new ConcurrentQueue<ProcessingCommand>();
            _aggregateRootId = aggregateRootId;
            _scheduler = scheduler;
            _messageHandler = messageHandler;
        }

        public void EnqueueMessage(ProcessingCommand message)
        {
            message.SetMailbox(this);
            _messageQueue.Enqueue(message);
            RegisterForExecution();
        }
        public bool EnterHandlingMessage()
        {
            return Interlocked.CompareExchange(ref _isHandlingMessage, 1, 0) == 0;
        }
        public void ExitHandlingMessage()
        {
            Interlocked.Exchange(ref _isHandlingMessage, 0);
        }
        public void CompleteMessage(ProcessingCommand processingMessage)
        {
            ExitHandlingMessage();
            RegisterForExecution();
        }
        public void Run()
        {
            ProcessingCommand processingMessage = null;
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
        private void RegisterForExecution()
        {
            _scheduler.ScheduleMailbox(this);
        }
    }
}
