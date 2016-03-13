using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ECommon.Extensions;
using ECommon.IO;
using ECommon.Logging;

namespace ENode.Commanding
{
    public class ProcessingCommandMailbox
    {
        #region Private Variables 

        private readonly ILogger _logger;
        private readonly object _lockObj = new object();
        private readonly object _lockObj2 = new object();
        private readonly string _aggregateRootId;
        private readonly ConcurrentDictionary<long, ProcessingCommand> _messageDict;
        private readonly Dictionary<long, CommandResult> _requestToCompleteOffsetDict;
        private readonly IProcessingCommandScheduler _scheduler;
        private readonly IProcessingCommandHandler _messageHandler;
        private long _maxOffset;
        private long _consumingOffset;
        private long _consumedOffset;
        private int _isHandlingMessage;
        private int _stopHandling;

        #endregion

        public string AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }

        public ProcessingCommandMailbox(string aggregateRootId, IProcessingCommandScheduler scheduler, IProcessingCommandHandler messageHandler, ILogger logger)
        {
            _messageDict = new ConcurrentDictionary<long, ProcessingCommand>();
            _requestToCompleteOffsetDict = new Dictionary<long, CommandResult>();
            _aggregateRootId = aggregateRootId;
            _scheduler = scheduler;
            _messageHandler = messageHandler;
            _logger = logger;
            _consumedOffset = -1;
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
        public void CompleteMessage(ProcessingCommand message, CommandResult commandResult)
        {
            lock (_lockObj2)
            {
                if (message.Sequence == _consumedOffset + 1)
                {
                    _messageDict.Remove(message.Sequence);
                    _consumedOffset = message.Sequence;
                    CompleteMessageWithResult(message, commandResult);
                    ProcessRequestToCompleteOffsets();
                }
                else if (message.Sequence > _consumedOffset + 1)
                {
                    _requestToCompleteOffsetDict[message.Sequence] = commandResult;
                }
                else if (message.Sequence < _consumedOffset + 1)
                {
                    _messageDict.Remove(message.Sequence);
                    _requestToCompleteOffsetDict.Remove(message.Sequence);
                }
            }
        }

        public void Run()
        {
            if (_stopHandling == 1)
            {
                return;
            }
            var hasException = false;
            ProcessingCommand processingMessage = null;
            try
            {
                if (HasRemainningMessage())
                {
                    processingMessage = GetNextMessage();
                    IncreaseConsumingOffset();

                    if (processingMessage != null)
                    {
                        _messageHandler.HandleAsync(processingMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                hasException = true;

                if (ex is IOException)
                {
                    //We need to retry the command.
                    DecreaseConsumingOffset();
                }

                if (processingMessage != null)
                {
                    var command = processingMessage.Message;
                    _logger.Error(string.Format("Failed to handle command [id: {0}, type: {1}]", command.Id, command.GetType().Name), ex);
                }
                else
                {
                    _logger.Error("Failed to run command mailbox.", ex);
                }
            }
            finally
            {
                if (hasException || processingMessage == null)
                {
                    ExitHandlingMessage();
                    if (HasRemainningMessage())
                    {
                        RegisterForExecution();
                    }
                }
            }
        }

        private void ProcessRequestToCompleteOffsets()
        {
            var nextSequence = _consumedOffset + 1;

            while (_requestToCompleteOffsetDict.ContainsKey(nextSequence))
            {
                var processingCommand = default(ProcessingCommand);
                if (_messageDict.TryRemove(nextSequence, out processingCommand))
                {
                    CompleteMessageWithResult(processingCommand, _requestToCompleteOffsetDict[nextSequence]);
                }
                _requestToCompleteOffsetDict.Remove(nextSequence);
                _consumedOffset = nextSequence;

                nextSequence++;
            }
        }
        private void CompleteMessageWithResult(ProcessingCommand processingCommand, CommandResult commandResult)
        {
            try
            {
                processingCommand.Complete(commandResult);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to complete command, commandId: {0}, aggregateRootId: {1}", processingCommand.Message.Id, processingCommand.Message.AggregateRootId), ex);
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
        private void DecreaseConsumingOffset()
        {
            _consumingOffset--;
        }
        private void RegisterForExecution()
        {
            _scheduler.ScheduleMailbox(this);
        }
    }
}
