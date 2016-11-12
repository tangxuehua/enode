using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Extensions;
using ECommon.Logging;
using ENode.Configurations;

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
        private readonly Dictionary<long, CommandResult> _requestToCompleteCommandDict;
        private readonly IProcessingCommandHandler _messageHandler;
        private readonly ManualResetEvent _pauseWaitHandle;
        private readonly ManualResetEvent _processingWaitHandle;
        private readonly int _batchSize;
        private long _nextSequence;
        private long _consumingSequence;
        private long _consumedSequence;
        private int _isRunning;
        private bool _isPaused;
        private bool _isProcessingCommand;

        #endregion

        public string AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }

        public ProcessingCommandMailbox(string aggregateRootId, IProcessingCommandHandler messageHandler, ILogger logger)
        {
            _messageDict = new ConcurrentDictionary<long, ProcessingCommand>();
            _requestToCompleteCommandDict = new Dictionary<long, CommandResult>();
            _pauseWaitHandle = new ManualResetEvent(false);
            _processingWaitHandle = new ManualResetEvent(false);
            _batchSize = ENodeConfiguration.Instance.Setting.CommandMailBoxPersistenceMaxBatchSize;
            _aggregateRootId = aggregateRootId;
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
            TryRun();
        }

        public void Pause()
        {
            _pauseWaitHandle.Reset();
            while (_isProcessingCommand)
            {
                _logger.InfoFormat("Request to pause the command mailbox, but the mailbox is currently processing command, so we should wait for a while, aggregateRootId: {0}", AggregateRootId);
                _processingWaitHandle.WaitOne(1000);
            }
            _isPaused = true;
        }
        public void Resume()
        {
            _isPaused = false;
            _pauseWaitHandle.Set();
            TryRun();
        }
        public void ResetConsumingSequence(long consumingSequence)
        {
            _consumingSequence = consumingSequence;
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
                        CompleteCommand(processingCommand, commandResult);
                        _consumedSequence = ProcessNextCompletedCommands(processingCommand.Sequence);
                    }
                    else if (processingCommand.Sequence > _consumedSequence + 1)
                    {
                        _requestToCompleteCommandDict[processingCommand.Sequence] = commandResult;
                    }
                    else if (processingCommand.Sequence < _consumedSequence + 1)
                    {
                        _messageDict.Remove(processingCommand.Sequence);
                        CompleteCommand(processingCommand, commandResult);
                        _requestToCompleteCommandDict.Remove(processingCommand.Sequence);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Command mailbox complete command failed, commandId: {0}, aggregateRootId: {1}", processingCommand.Message.Id, processingCommand.Message.AggregateRootId), ex);
                }
            }
        }
        public void Run()
        {
            while (_isPaused)
            {
                _logger.InfoFormat("Command mailbox is pausing and we should wait for a while, aggregateRootId: {0}", AggregateRootId);
                _pauseWaitHandle.WaitOne(1000);
            }
            ProcessingCommand processingCommand = null;
            try
            {
                _processingWaitHandle.Reset();
                _isProcessingCommand = true;
                var count = 0;
                while (_consumingSequence < _nextSequence && count < _batchSize)
                {
                    processingCommand = GetProcessingCommand(_consumingSequence);
                    if (processingCommand != null)
                    {
                        _messageHandler.Handle(processingCommand);
                    }
                    _consumingSequence++;
                    count++;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Command mailbox run has unknown exception, aggregateRootId: {0}, commandId: {1}", AggregateRootId, processingCommand != null ? processingCommand.Message.Id : string.Empty), ex);
                Thread.Sleep(1);
            }
            finally
            {
                _isProcessingCommand = false;
                _processingWaitHandle.Set();
                Exit();
                if (_consumingSequence < _nextSequence)
                {
                    TryRun();
                }
            }
        }

        private ProcessingCommand GetProcessingCommand(long sequence)
        {
            ProcessingCommand processingMessage;
            if (_messageDict.TryGetValue(sequence, out processingMessage))
            {
                return processingMessage;
            }
            return null;
        }
        private long ProcessNextCompletedCommands(long baseSequence)
        {
            var returnSequence = baseSequence;
            var nextSequence = baseSequence + 1;
            while (_requestToCompleteCommandDict.ContainsKey(nextSequence))
            {
                var processingCommand = default(ProcessingCommand);
                if (_messageDict.TryRemove(nextSequence, out processingCommand))
                {
                    var commandResult = _requestToCompleteCommandDict[nextSequence];
                    CompleteCommand(processingCommand, commandResult);
                }
                _requestToCompleteCommandDict.Remove(nextSequence);
                returnSequence = nextSequence;
                nextSequence++;
            }
            return returnSequence;
        }
        private void CompleteCommand(ProcessingCommand processingCommand, CommandResult commandResult)
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
        private void TryRun()
        {
            if (TryEnter())
            {
                Task.Factory.StartNew(Run);
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
