using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Extensions;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Scheduling;
using ECommon.Serializing;
using ENode.Commanding;

namespace ENode.EQueue
{
    public class CommandResultProcessor : IRequestHandler
    {
        private byte[] ByteArray = new byte[0];
        private SocketRemotingServer _remotingServer;
        private ConcurrentDictionary<string, CommandTaskCompletionSource> _commandTaskDict;
        private BlockingCollection<CommandResult> _commandExecutedMessageLocalQueue;
        private BlockingCollection<DomainEventHandledMessage> _domainEventHandledMessageLocalQueue;
        private Worker _commandExecutedMessageWorker;
        private Worker _domainEventHandledMessageWorker;
        private IJsonSerializer _jsonSerializer;
        private ILogger _logger;
        private bool _started;

        public IPEndPoint BindingAddress { get; private set; }

        public CommandResultProcessor Initialize(IPEndPoint bindingAddress)
        {
            _remotingServer = new SocketRemotingServer("CommandResultProcessor.RemotingServer", bindingAddress);
            _commandTaskDict = new ConcurrentDictionary<string, CommandTaskCompletionSource>();
            _commandExecutedMessageLocalQueue = new BlockingCollection<CommandResult>(new ConcurrentQueue<CommandResult>());
            _domainEventHandledMessageLocalQueue = new BlockingCollection<DomainEventHandledMessage>(new ConcurrentQueue<DomainEventHandledMessage>());
            _commandExecutedMessageWorker = new Worker("ProcessExecutedCommandMessage", () => ProcessExecutedCommandMessage(_commandExecutedMessageLocalQueue.Take()));
            _domainEventHandledMessageWorker = new Worker("ProcessDomainEventHandledMessage", () => ProcessDomainEventHandledMessage(_domainEventHandledMessageLocalQueue.Take()));
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            BindingAddress = bindingAddress;
            return this;
        }

        public void RegisterProcessingCommand(ICommand command, CommandReturnType commandReturnType, TaskCompletionSource<CommandResult> taskCompletionSource)
        {
            if (!_commandTaskDict.TryAdd(command.Id, new CommandTaskCompletionSource { CommandReturnType = commandReturnType, TaskCompletionSource = taskCompletionSource }))
            {
                throw new Exception(string.Format("Duplicate processing command registration, type:{0}, id:{1}", command.GetType().Name, command.Id));
            }
        }
        public void ProcessFailedSendingCommand(ICommand command)
        {
            if (_commandTaskDict.TryRemove(command.Id, out CommandTaskCompletionSource commandTaskCompletionSource))
            {
                var commandResult = new CommandResult(CommandStatus.Failed, command.Id, command.AggregateRootId, "Failed to send the command.", typeof(string).FullName);
                commandTaskCompletionSource.TaskCompletionSource.TrySetResult(commandResult);
            }
        }

        public CommandResultProcessor Start()
        {
            if (_started) return this;

            _remotingServer.Start();
            _commandExecutedMessageWorker.Start();
            _domainEventHandledMessageWorker.Start();

            _remotingServer.RegisterRequestHandler((int)CommandReturnType.CommandExecuted, this);
            _remotingServer.RegisterRequestHandler((int)CommandReturnType.EventHandled, this);

            _started = true;

            return this;
        }
        public CommandResultProcessor Shutdown()
        {
            _remotingServer.Shutdown();
            _commandExecutedMessageWorker.Stop();
            _domainEventHandledMessageWorker.Stop();
            return this;
        }

        RemotingResponse IRequestHandler.HandleRequest(IRequestHandlerContext context, RemotingRequest remotingRequest)
        {
            if (remotingRequest.Code == (int)CommandReturnType.CommandExecuted)
            {
                var json = Encoding.UTF8.GetString(remotingRequest.Body);
                var result = _jsonSerializer.Deserialize<CommandResult>(json);
                _commandExecutedMessageLocalQueue.Add(result);
            }
            else if (remotingRequest.Code == (int)CommandReturnType.EventHandled)
            {
                var json = Encoding.UTF8.GetString(remotingRequest.Body);
                var message = _jsonSerializer.Deserialize<DomainEventHandledMessage>(json);
                _domainEventHandledMessageLocalQueue.Add(message);
            }
            else
            {
                _logger.ErrorFormat("Invalid remoting request code: {0}", remotingRequest.Code);
            }
            return null;
        }

        private void ProcessExecutedCommandMessage(CommandResult commandResult)
        {
            if (_commandTaskDict.TryGetValue(commandResult.CommandId, out CommandTaskCompletionSource commandTaskCompletionSource))
            {
                if (commandTaskCompletionSource.CommandReturnType == CommandReturnType.CommandExecuted)
                {
                    _commandTaskDict.Remove(commandResult.CommandId);
                    if (commandTaskCompletionSource.TaskCompletionSource.TrySetResult(commandResult))
                    {
                        _logger.InfoFormat("Command result return, {0}", commandResult);
                    }
                }
                else if (commandTaskCompletionSource.CommandReturnType == CommandReturnType.EventHandled)
                {
                    if (commandResult.Status == CommandStatus.Failed || commandResult.Status == CommandStatus.NothingChanged)
                    {
                        _commandTaskDict.Remove(commandResult.CommandId);
                        if (commandTaskCompletionSource.TaskCompletionSource.TrySetResult(commandResult))
                        {
                            _logger.InfoFormat("Command result return, {0}", commandResult);
                        }
                    }
                }
            }
        }
        private void ProcessDomainEventHandledMessage(DomainEventHandledMessage message)
        {
            if (_commandTaskDict.TryRemove(message.CommandId, out CommandTaskCompletionSource commandTaskCompletionSource))
            {
                var commandResult = new CommandResult(CommandStatus.Success, message.CommandId, message.AggregateRootId, message.CommandResult, message.CommandResult != null ? typeof(string).FullName : null);
                if (commandTaskCompletionSource.TaskCompletionSource.TrySetResult(commandResult))
                {
                    _logger.InfoFormat("Command result return, {0}", commandResult);
                }
            }
        }

        class CommandTaskCompletionSource
        {
            public TaskCompletionSource<CommandResult> TaskCompletionSource { get; set; }
            public CommandReturnType CommandReturnType { get; set; }
        }
    }
}
