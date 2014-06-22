using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.Components;
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
        private const string DefaultCommandExecutedMessageConsumerId = "sys_cemc";
        private const string DefaultCommandExecutedMessageConsumerGroup = "sys_cemcg";
        private const string DefaultDomainEventHandledMessageConsumerId = "sys_dehmc";
        private const string DefaultDomainEventHandledMessageConsumerGroup = "sys_dehmcg";
        private const string DefaultCommandExecutedMessageTopic = "sys_ecmt";
        private const string DefaultDomainEventHandledMessageTopic = "sys_dehmt";

        private readonly Consumer _commandExecutedMessageConsumer;
        private readonly Consumer _domainEventHandledMessageConsumer;
        private readonly ConcurrentDictionary<string, CommandTaskCompletionSource> _commandTaskDict;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ProcessResult>> _processTaskDict;
        private readonly BlockingCollection<CommandExecutedMessage> _commandExecutedMessageLocalQueue;
        private readonly BlockingCollection<DomainEventHandledMessage> _domainEventHandledMessageLocalQueue;
        private readonly Worker _commandExecutedMessageWorker;
        private readonly Worker _domainEventHandledMessageWorker;
        private readonly IBinarySerializer _binarySerializer;
        private bool _started;

        public string CommandExecutedMessageTopic { get; private set; }
        public string DomainEventHandledMessageTopic { get; private set; }
        public Consumer CommandExecutedMessageConsumer { get { return _commandExecutedMessageConsumer; } }
        public Consumer DomainEventHandledMessageConsumer { get { return _domainEventHandledMessageConsumer; } }

        public CommandResultProcessor() : this(
            new Consumer(DefaultCommandExecutedMessageConsumerId, DefaultCommandExecutedMessageConsumerGroup),
            new Consumer(DefaultDomainEventHandledMessageConsumerId, DefaultDomainEventHandledMessageConsumerGroup))
        {
        }
        public CommandResultProcessor(Consumer commandExecutedMessageConsumer, Consumer domainEventHandledMessageConsumer)
        {
            _commandExecutedMessageConsumer = commandExecutedMessageConsumer;
            _domainEventHandledMessageConsumer = domainEventHandledMessageConsumer;
            _commandTaskDict = new ConcurrentDictionary<string, CommandTaskCompletionSource>();
            _processTaskDict = new ConcurrentDictionary<string, TaskCompletionSource<ProcessResult>>();
            _commandExecutedMessageLocalQueue = new BlockingCollection<CommandExecutedMessage>(new ConcurrentQueue<CommandExecutedMessage>());
            _domainEventHandledMessageLocalQueue = new BlockingCollection<DomainEventHandledMessage>(new ConcurrentQueue<DomainEventHandledMessage>());
            _commandExecutedMessageWorker = new Worker("ProcessExecutedCommandMessage", () => ProcessExecutedCommandMessage(_commandExecutedMessageLocalQueue.Take()));
            _domainEventHandledMessageWorker = new Worker("ProcessDomainEventHandledMessage", () => ProcessDomainEventHandledMessage(_domainEventHandledMessageLocalQueue.Take()));
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            CommandExecutedMessageTopic = DefaultCommandExecutedMessageTopic;
            DomainEventHandledMessageTopic = DefaultDomainEventHandledMessageTopic;
        }

        public CommandResultProcessor SetExecutedCommandMessageTopic(string topic)
        {
            CommandExecutedMessageTopic = topic;
            return this;
        }
        public CommandResultProcessor SetDomainEventHandledMessageTopic(string topic)
        {
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
                commandTaskCompletionSource.TaskCompletionSource.TrySetResult(
                    new CommandResult(
                        CommandStatus.Failed,
                        command.Id,
                        command.AggregateRootId,
                        "CommandSendFailed",
                        "Failed to send the command.",
                        command.Items));
            }
            return this;
        }
        public CommandResultProcessor NotifyProcessCommandSendFailed(IProcessCommand command)
        {
            TaskCompletionSource<ProcessResult> taskCompletionSource;
            if (_processTaskDict.TryGetValue(command.ProcessId, out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(
                    new ProcessResult(
                        command.ProcessId,
                        ProcessStatus.Failed,
                        0,
                        "ProcessCommandSendFailed",
                        "Failed to send the process command.",
                        command.Items));
            }
            return this;
        }

        public CommandResultProcessor Start()
        {
            if (_started) return this;

            if (string.IsNullOrEmpty(CommandExecutedMessageTopic))
            {
                throw new Exception("Command result processor cannot start as the command executed message topic is not set.");
            }
            if (string.IsNullOrEmpty(DomainEventHandledMessageTopic))
            {
                throw new Exception("Command result processor cannot start as the domain event handled message topic is not set.");
            }

            _commandExecutedMessageConsumer.Subscribe(CommandExecutedMessageTopic);
            _domainEventHandledMessageConsumer.Subscribe(DomainEventHandledMessageTopic);

            _commandExecutedMessageConsumer.SetMessageHandler(new CommandExecutedMessageHandler(this)).Start();
            _domainEventHandledMessageConsumer.SetMessageHandler(new DomainEventHandledMessageHandler(this)).Start();

            _commandExecutedMessageWorker.Start();
            _domainEventHandledMessageWorker.Start();

            _started = true;

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
                    commandTaskCompletionSource.TaskCompletionSource.TrySetResult(
                        new CommandResult(
                            message.CommandStatus,
                            message.CommandId,
                            message.AggregateRootId,
                            message.ExceptionTypeName,
                            message.ErrorMessage,
                            message.Items));
                }
                else if (commandTaskCompletionSource.CommandReturnType == CommandReturnType.EventHandled)
                {
                    if (message.CommandStatus == CommandStatus.Failed || message.CommandStatus == CommandStatus.NothingChanged)
                    {
                        commandTaskCompletionSource.TaskCompletionSource.TrySetResult(
                            new CommandResult(
                                message.CommandStatus,
                                message.CommandId,
                                message.AggregateRootId,
                                message.ExceptionTypeName,
                                message.ErrorMessage,
                                message.Items));
                    }
                }
            }
            if (message.CommandStatus == CommandStatus.Failed && !string.IsNullOrEmpty(message.ProcessId))
            {
                TaskCompletionSource<ProcessResult> processTaskCompletionSource;
                if (_processTaskDict.TryGetValue(message.ProcessId, out processTaskCompletionSource))
                {
                    processTaskCompletionSource.TrySetResult(
                        new ProcessResult(
                            message.ProcessId,
                            ProcessStatus.Failed,
                            0,
                            message.ExceptionTypeName,
                            message.ErrorMessage,
                            message.Items));
                }
            }
        }
        private void ProcessDomainEventHandledMessage(DomainEventHandledMessage message)
        {
            CommandTaskCompletionSource commandTaskCompletionSource;
            if (_commandTaskDict.TryGetValue(message.CommandId, out commandTaskCompletionSource))
            {
                commandTaskCompletionSource.TaskCompletionSource.TrySetResult(
                    new CommandResult(
                        CommandStatus.Success,
                        message.CommandId,
                        message.AggregateRootId,
                        null,
                        null,
                        message.Items));
            }
            if (message.IsProcessCompleted && !string.IsNullOrEmpty(message.ProcessId))
            {
                TaskCompletionSource<ProcessResult> processTaskCompletionSource;
                if (_processTaskDict.TryGetValue(message.ProcessId, out processTaskCompletionSource))
                {
                    if (message.IsProcessSuccess)
                    {
                        processTaskCompletionSource.TrySetResult(
                            new ProcessResult(
                                message.ProcessId,
                                ProcessStatus.Success,
                                0,
                                null,
                                null,
                                message.Items));
                    }
                    else
                    {
                        processTaskCompletionSource.TrySetResult(
                            new ProcessResult(
                                message.ProcessId,
                                ProcessStatus.Failed,
                                message.ErrorCode,
                                null,
                                null,
                                message.Items));
                    }
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
