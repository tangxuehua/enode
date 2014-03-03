using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.IoC;
using ECommon.Scheduling;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue.Commanding
{
    public class CommandResultProcessor
    {
        private readonly Consumer _commandExecutedMessageConsumer;
        private readonly Consumer _domainEventHandledMessageConsumer;
        private readonly ConcurrentDictionary<string, CommandTaskCompletionSource> _commandTaskDict;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ProcessResult>> _processTaskDict;
        private readonly BlockingCollection<CommandExecutedMessage> _commandExecutedMessageLocalQueue;
        private readonly BlockingCollection<DomainEventHandledMessage> _domainEventHandledMessageLocalQueue;
        private readonly Worker _commandExecutedMessageWorker;
        private readonly Worker _domainEventHandledMessageWorker;
        private readonly IBinarySerializer _binarySerializer;

        public string CommandExecutedMessageTopic { get; private set; }
        public string DomainEventHandledMessageTopic { get; private set; }
        public Consumer CommandExecutedMessageConsumer { get { return _commandExecutedMessageConsumer; } }
        public Consumer DomainEventHandledMessageConsumer { get { return _domainEventHandledMessageConsumer; } }

        public CommandResultProcessor(Consumer commandExecutedMessageConsumer, Consumer domainEventHandledMessageConsumer)
        {
            _commandExecutedMessageConsumer = commandExecutedMessageConsumer;
            _domainEventHandledMessageConsumer = domainEventHandledMessageConsumer;
            _commandTaskDict = new ConcurrentDictionary<string, CommandTaskCompletionSource>();
            _processTaskDict = new ConcurrentDictionary<string, TaskCompletionSource<ProcessResult>>();
            _commandExecutedMessageLocalQueue = new BlockingCollection<CommandExecutedMessage>(new ConcurrentQueue<CommandExecutedMessage>());
            _domainEventHandledMessageLocalQueue = new BlockingCollection<DomainEventHandledMessage>(new ConcurrentQueue<DomainEventHandledMessage>());
            _commandExecutedMessageWorker = new Worker(() => ProcessExecutedCommandMessage(_commandExecutedMessageLocalQueue.Take()));
            _domainEventHandledMessageWorker = new Worker(() => ProcessDomainEventHandledMessage(_domainEventHandledMessageLocalQueue.Take()));
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        public CommandResultProcessor SetExecutedCommandMessageTopic(string topic)
        {
            if (CommandExecutedMessageTopic != null)
            {
                throw new ENodeException("Executed command message topic can't be set twice.");
            }
            _commandExecutedMessageConsumer.Subscribe(topic);
            CommandExecutedMessageTopic = topic;
            return this;
        }
        public CommandResultProcessor SetDomainEventHandledMessageTopic(string topic)
        {
            if (DomainEventHandledMessageTopic != null)
            {
                throw new ENodeException("Domain event handled message topic can't be set twice.");
            }
            _domainEventHandledMessageConsumer.Subscribe(topic);
            DomainEventHandledMessageTopic = topic;
            return this;
        }

        public CommandResultProcessor RegisterCommand(ICommand command, CommandReturnType commandReturnType, TaskCompletionSource<CommandResult> taskCompletionSource)
        {
            _commandTaskDict.TryAdd(command.Id, new CommandTaskCompletionSource { CommandReturnType = commandReturnType, TaskCompletionSource = taskCompletionSource });
            return this;
        }
        public CommandResultProcessor RegisterProcess(IProcessCommand command, TaskCompletionSource<ProcessResult> taskCompletionSource)
        {
            _processTaskDict.TryAdd(command.ProcessId, taskCompletionSource);
            return this;
        }

        public CommandResultProcessor NotifyCommandSendFailed(ICommand command)
        {
            CommandTaskCompletionSource commandTaskCompletionSource;
            if (_commandTaskDict.TryGetValue(command.Id, out commandTaskCompletionSource))
            {
                commandTaskCompletionSource.TaskCompletionSource.TrySetResult(new CommandResult(CommandStatus.Failed, command.Id, command.AggregateRootId, 0, "Command send failed."));
            }
            return this;
        }
        public CommandResultProcessor NotifyProcessCommandSendFailed(IProcessCommand command)
        {
            TaskCompletionSource<ProcessResult> taskCompletionSource;
            if (_processTaskDict.TryGetValue(command.ProcessId, out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(new ProcessResult(command.ProcessId, 0, "Start process command send failed."));
            }
            return this;
        }

        public CommandResultProcessor Start()
        {
            _commandExecutedMessageConsumer.Start(new CommandExecutedMessageHandler(this));
            _domainEventHandledMessageConsumer.Start(new DomainEventHandledMessageHandler(this));
            _commandExecutedMessageWorker.Start();
            _domainEventHandledMessageWorker.Start();
            return this;
        }
        public CommandResultProcessor Shutdown()
        {
            _commandExecutedMessageConsumer.Shutdown();
            _domainEventHandledMessageConsumer.Shutdown();
            _commandExecutedMessageWorker.Stop();
            _domainEventHandledMessageWorker.Stop();
            return this;
        }

        private void ProcessExecutedCommandMessage(CommandExecutedMessage message)
        {
            CommandTaskCompletionSource commandTaskCompletionSource;
            if (_commandTaskDict.TryGetValue(message.CommandId, out commandTaskCompletionSource))
            {
                if (commandTaskCompletionSource.CommandReturnType == CommandReturnType.CommandExecuted)
                {
                    commandTaskCompletionSource.TaskCompletionSource.TrySetResult(new CommandResult(message.CommandStatus, message.CommandId, message.AggregateRootId, message.ExceptionCode, message.ErrorMessage));
                }
                if (commandTaskCompletionSource.CommandReturnType == CommandReturnType.DomainEventHandled)
                {
                    if (message.CommandStatus == CommandStatus.Failed || message.CommandStatus == CommandStatus.NothingChanged)
                    {
                        commandTaskCompletionSource.TaskCompletionSource.TrySetResult(new CommandResult(message.CommandStatus, message.CommandId, message.AggregateRootId, message.ExceptionCode, message.ErrorMessage));
                    }
                }
            }
            if (!string.IsNullOrEmpty(message.ProcessId))
            {
                if (message.CommandStatus == CommandStatus.Failed)
                {
                    TaskCompletionSource<ProcessResult> processTaskCompletionSource;
                    if (_processTaskDict.TryGetValue(message.ProcessId, out processTaskCompletionSource))
                    {
                        processTaskCompletionSource.TrySetResult(new ProcessResult(message.ProcessId, message.ExceptionCode, message.ErrorMessage));
                    }
                }
            }
        }
        private void ProcessDomainEventHandledMessage(DomainEventHandledMessage message)
        {
            CommandTaskCompletionSource commandTaskCompletionSource;
            if (_commandTaskDict.TryGetValue(message.CommandId, out commandTaskCompletionSource))
            {
                commandTaskCompletionSource.TaskCompletionSource.TrySetResult(new CommandResult(CommandStatus.Success, message.CommandId, message.AggregateRootId, 0, null));
            }
            if (message.IsProcessCompletedEvent && !string.IsNullOrEmpty(message.ProcessId))
            {
                TaskCompletionSource<ProcessResult> processTaskCompletionSource;
                if (_processTaskDict.TryGetValue(message.ProcessId, out processTaskCompletionSource))
                {
                    processTaskCompletionSource.TrySetResult(new ProcessResult(message.ProcessId));
                }
            }
        }

        class CommandTaskCompletionSource
        {
            public TaskCompletionSource<CommandResult> TaskCompletionSource { get; set; }
            public CommandReturnType CommandReturnType { get; set; }
        }
        class CommandExecutedMessageHandler : IMessageHandler
        {
            private CommandResultProcessor _processor;

            public CommandExecutedMessageHandler(CommandResultProcessor processor)
            {
                _processor = processor;
            }

            void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
            {
                _processor._commandExecutedMessageLocalQueue.Add(_processor._binarySerializer.Deserialize(message.Body, typeof(CommandExecutedMessage)) as CommandExecutedMessage);
                context.OnMessageHandled(message);
            }
        }
        class DomainEventHandledMessageHandler : IMessageHandler
        {
            private CommandResultProcessor _processor;

            public DomainEventHandledMessageHandler(CommandResultProcessor processor)
            {
                _processor = processor;
            }

            void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
            {
                _processor._domainEventHandledMessageLocalQueue.Add(_processor._binarySerializer.Deserialize(message.Body, typeof(DomainEventHandledMessage)) as DomainEventHandledMessage);
                context.OnMessageHandled(message);
            }
        }
    }
}
