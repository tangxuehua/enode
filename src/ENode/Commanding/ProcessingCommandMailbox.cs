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
        private readonly Dictionary<long, CommandResult> _requestToCompleteSequenceDict;
        private readonly IProcessingCommandScheduler _scheduler;
        private readonly IProcessingCommandHandler _messageHandler;
        private long _nextSequence;
        private long _consumingSequence;
        private long _consumedSequence;
        private int _isHandlingMessage;
        private int _isPaused;

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
            _requestToCompleteSequenceDict = new Dictionary<long, CommandResult>();
            _aggregateRootId = aggregateRootId;
            _scheduler = scheduler;
            _messageHandler = messageHandler;
            _logger = logger;
            _consumedSequence = -1;
        }

        public void EnqueueMessage(ProcessingCommand message)
        {
            lock (_lockObj)
            {
                message.Sequence = _nextSequence;
                message.Mailbox = this;
                if (_messageDict.TryAdd(message.Sequence, message))
                {
                    _nextSequence++;
                }
            }
            RegisterForExecution();
        }
        public bool EnterHandlingMessage()
        {
            return Interlocked.CompareExchange(ref _isHandlingMessage, 1, 0) == 0;
        }
        public void PauseHandlingMessage()
        {
            _isPaused = 1;
        }
        public void ResetConsumingSequence(long consumingSequence)
        {
            _consumingSequence = consumingSequence;
        }
        public void ResumeHandlingMessage()
        {
            _isPaused = 0;
            RegisterForExecution();
        }
        public void TryExecuteNextMessage()
        {
            ExitHandlingMessage();
            RegisterForExecution();
        }
        public void CompleteMessage(ProcessingCommand processingCommand, CommandResult commandResult)
        {
            lock (_lockObj2)
            {
                try
                {
                    if (processingCommand.Sequence == _consumedSequence + 1)
                    {
                        _messageDict.Remove(processingCommand.Sequence);
                        CompleteCommandWithResult(processingCommand, commandResult);
                        _consumedSequence = ProcessNextCompletedCommands(processingCommand.Sequence);
                    }
                    else if (processingCommand.Sequence > _consumedSequence + 1)
                    {
                        _requestToCompleteSequenceDict[processingCommand.Sequence] = commandResult;
                    }
                    else if (processingCommand.Sequence < _consumedSequence + 1)
                    {
                        _messageDict.Remove(processingCommand.Sequence);
                        CompleteCommandWithResult(processingCommand, commandResult);
                        _requestToCompleteSequenceDict.Remove(processingCommand.Sequence);
                    }
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Command mailbox complete command success, commandId: {0}, aggregateRootId: {1}", processingCommand.Message.Id, processingCommand.Message.AggregateRootId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Command mailbox complete command failed, commandId: {0}, aggregateRootId: {1}", processingCommand.Message.Id, processingCommand.Message.AggregateRootId), ex);
                }
                finally
                {
                    RegisterForExecution();
                }
            }
        }

        public void Run()
        {
            if (_isPaused == 1)
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
                    IncreaseConsumingSequence();

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
                    DecreaseConsumingSequence();
                }

                if (processingMessage != null)
                {
                    var command = processingMessage.Message;
                    _logger.Error(string.Format("Failed to handle command [id: {0}, type: {1}], aggregateId: {2}", command.Id, command.GetType().Name, AggregateRootId), ex);
                }
                else
                {
                    _logger.Error(string.Format("Failed to run command mailbox, aggregateId: {0}", AggregateRootId), ex);
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

        private long ProcessNextCompletedCommands(long baseSequence)
        {
            var returnSequence = baseSequence;
            var nextSequence = baseSequence + 1;
            while (_requestToCompleteSequenceDict.ContainsKey(nextSequence))
            {
                var processingCommand = default(ProcessingCommand);
                if (_messageDict.TryRemove(nextSequence, out processingCommand))
                {
                    CompleteCommandWithResult(processingCommand, _requestToCompleteSequenceDict[nextSequence]);
                }
                _requestToCompleteSequenceDict.Remove(nextSequence);
                returnSequence = nextSequence;
                nextSequence++;
            }
            return returnSequence;
        }
        private void CompleteCommandWithResult(ProcessingCommand processingCommand, CommandResult commandResult)
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
            return _consumingSequence < _nextSequence;
        }
        private ProcessingCommand GetNextMessage()
        {
            ProcessingCommand processingMessage;
            if (_messageDict.TryGetValue(_consumingSequence, out processingMessage))
            {
                return processingMessage;
            }
            return null;
        }
        private void IncreaseConsumingSequence()
        {
            _consumingSequence++;
        }
        private void DecreaseConsumingSequence()
        {
            _consumingSequence--;
        }
        private void RegisterForExecution()
        {
            _scheduler.ScheduleMailbox(this);
        }
    }
}
