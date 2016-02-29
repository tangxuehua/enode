using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ECommon.Extensions;

namespace ENode.Commanding
{
    public class ProcessingCommandMailbox
    {
        private readonly object _lockObj = new object();
        private readonly string _aggregateRootId;
        private readonly ConcurrentDictionary<long, ProcessingCommand> _messageDict;
        private readonly IProcessingCommandScheduler _scheduler;
        private readonly IProcessingCommandHandler _messageHandler;
        private long _maxOffset;
        private long _consumingOffset;
        private int _isHandlingMessage;
        private int _stopHandling;

        public string AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }

        public ProcessingCommandMailbox(string aggregateRootId, IProcessingCommandScheduler scheduler, IProcessingCommandHandler messageHandler)
        {
            _messageDict = new ConcurrentDictionary<long, ProcessingCommand>();
            _aggregateRootId = aggregateRootId;
            _scheduler = scheduler;
            _messageHandler = messageHandler;
        }

        public void EnqueueMessage(ProcessingCommand message)
        {
            lock (_lockObj)
            {
                message.Sequence = _maxOffset;
                message.Mailbox = this;
                _messageDict.TryAdd(message.Sequence, message);
                _maxOffset++;
            }
            RegisterForExecution();
        }
        public bool EnterHandlingMessage()
        {
            return Interlocked.CompareExchange(ref _isHandlingMessage, 1, 0) == 0;
        }
        public void StopHandlingMessage()
        {
            _stopHandling = 1;
        }
        public void ResetConsumingOffset(long consumingOffset)
        {
            _consumingOffset = consumingOffset;
        }
        public void RestartHandlingMessage()
        {
            _stopHandling = 0;
            TryExecuteNextMessage();
        }
        public void TryExecuteNextMessage()
        {
            ExitHandlingMessage();
            RegisterForExecution();
        }
        public void RemoveCompleteMessage(ProcessingCommand message)
        {
            _messageDict.Remove(message.Sequence);
        }
        public void RemoveCompleteMessages(IEnumerable<ProcessingCommand> messages)
        {
            foreach (var message in messages)
            {
                _messageDict.Remove(message.Sequence);
            }
        }

        public void Run()
        {
            if (_stopHandling == 1)
            {
                return;
            }
            ProcessingCommand processingMessage = null;
            try
            {
                if (HasRemainningMessage())
                {
                    processingMessage = GetNextMessage();
                    if (processingMessage != null)
                    {
                        _messageHandler.HandleAsync(processingMessage);
                    }
                    else
                    {
                        IncreaseConsumingOffset();
                    }
                }
            }
            finally
            {
                if (processingMessage == null)
                {
                    ExitHandlingMessage();
                    if (HasRemainningMessage())
                    {
                        RegisterForExecution();
                    }
                }
                else
                {
                    IncreaseConsumingOffset();
                }
            }
        }

        private void ExitHandlingMessage()
        {
            Interlocked.Exchange(ref _isHandlingMessage, 0);
        }
        private bool HasRemainningMessage()
        {
            return _consumingOffset < _maxOffset;
        }
        private ProcessingCommand GetNextMessage()
        {
            ProcessingCommand processingMessage;
            if (_messageDict.TryGetValue(_consumingOffset, out processingMessage))
            {
                return processingMessage;
            }
            return null;
        }
        private void IncreaseConsumingOffset()
        {
            _consumingOffset++;
        }
        private void RegisterForExecution()
        {
            _scheduler.ScheduleMailbox(this);
        }
    }
}
