using System;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.EQueue.Commanding;
using EQueue.Clients.Producers;
using EQueue.Protocols;
using EQueue.Utils;

namespace ENode.EQueue
{
    public class CommandService : ICommandService, IProcessCommandSender
    {
        private const string DefaultCommandExecutedMessageTopic = "sys_ecmt";
        private const string DefaultDomainEventHandledMessageTopic = "sys_dehmt";
        private const string DefaultCommandServiceProcuderId = "sys_csp";
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ICommandTopicProvider _commandTopicProvider;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly ICommandRouteKeyProvider _commandRouteKeyProvider;
        private readonly CommandResultProcessor _commandResultProcessor;
        private readonly Producer _producer;

        public string CommandExecutedMessageTopic { get; private set; }
        public string DomainEventHandledMessageTopic { get; private set; }

        public CommandService(CommandResultProcessor commandResultProcessor = null, string id = null, ProducerSetting setting = null)
        {
            _commandResultProcessor = commandResultProcessor;
            _producer = new Producer(id ?? DefaultCommandServiceProcuderId, setting ?? new ProducerSetting());
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTopicProvider = ObjectContainer.Resolve<ICommandTopicProvider>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
            _commandRouteKeyProvider = ObjectContainer.Resolve<ICommandRouteKeyProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            CommandExecutedMessageTopic = DefaultCommandExecutedMessageTopic;
            DomainEventHandledMessageTopic = DefaultDomainEventHandledMessageTopic;
        }

        public CommandService Start()
        {
            _producer.Start();
            if (_commandResultProcessor != null)
            {
                _commandResultProcessor.Start();
            }
            return this;
        }
        public CommandService Shutdown()
        {
            _producer.Shutdown();
            if (_commandResultProcessor != null)
            {
                _commandResultProcessor.Shutdown();
            }
            return this;
        }
        public void Send(ICommand command)
        {
            ValidateCommand(command);
            var result = _producer.Send(BuildCommandMessage(command), _commandRouteKeyProvider.GetRouteKey(command));
            if (result.SendStatus == SendStatus.Failed)
            {
                throw new CommandSendException(result.ErrorMessage);
            }
        }
        public void Send(IProcessCommand processCommand, string sourceEventId)
        {
            ValidateCommand(processCommand);
            if (string.IsNullOrEmpty(sourceEventId))
            {
                throw new ArgumentException("Source event id can not be null or empty.");
            }
            var result = _producer.Send(BuildCommandMessage(processCommand, sourceEventId), _commandRouteKeyProvider.GetRouteKey(processCommand));
            if (result.SendStatus == SendStatus.Failed)
            {
                throw new CommandSendException(result.ErrorMessage);
            }
        }
        public Task<CommandSendResult> SendAsync(ICommand command)
        {
            ValidateCommand(command);
            var taskCompletionSource = new TaskCompletionSource<CommandSendResult>();

            _producer.SendAsync(BuildCommandMessage(command), _commandRouteKeyProvider.GetRouteKey(command)).ContinueWith(sendTask =>
            {
                taskCompletionSource.TrySetResult(
                    new CommandSendResult(
                        sendTask.Result.SendStatus == SendStatus.Success ? CommandSendStatus.Success : CommandSendStatus.Failed,
                        sendTask.Result.ErrorMessage));
            });

            return taskCompletionSource.Task;
        }
        public Task<CommandResult> Execute(ICommand command)
        {
            return Execute(command, CommandReturnType.EventHandled);
        }
        public Task<CommandResult> Execute(ICommand command, CommandReturnType commandReturnType)
        {
            if (_commandResultProcessor == null)
            {
                throw new NotSupportedException("Not supported operation as the command result processor is not set.");
            }

            ValidateCommand(command);
            var taskCompletionSource = new TaskCompletionSource<CommandResult>();

            if (!_commandResultProcessor.RegisterCommand(command, commandReturnType, taskCompletionSource))
            {
                throw new Exception("Duplicate command as there already has a command with the same command id is being executing.");
            }

            _producer.SendAsync(BuildCommandMessage(command), _commandRouteKeyProvider.GetRouteKey(command)).ContinueWith(sendTask =>
            {
                if (sendTask.Result.SendStatus == SendStatus.Failed)
                {
                    _commandResultProcessor.NotifyCommandSendFailed(command);
                }
            });

            return taskCompletionSource.Task;
        }
        public Task<ProcessResult> StartProcess(IProcessCommand command)
        {
            if (_commandResultProcessor == null)
            {
                throw new NotSupportedException("Not supported operation as the command result processor is not set.");
            }

            ValidateCommand(command);
            var taskCompletionSource = new TaskCompletionSource<ProcessResult>();

            if (!_commandResultProcessor.RegisterProcess(command, taskCompletionSource))
            {
                throw new Exception("Duplicate process as there already has a process with the same process id is being processing.");
            }

            _producer.SendAsync(BuildCommandMessage(command), _commandRouteKeyProvider.GetRouteKey(command)).ContinueWith(sendTask =>
            {
                if (sendTask.Result.SendStatus == SendStatus.Failed)
                {
                    _commandResultProcessor.NotifyProcessCommandSendFailed(command);
                }
            });

            return taskCompletionSource.Task;
        }

        private void ValidateCommand(ICommand command)
        {
            if (string.IsNullOrEmpty(command.Id))
            {
                throw new ArgumentException("Command id can not be null or empty.");
            }
            if (!(command is ICreatingAggregateCommand) && string.IsNullOrEmpty(command.AggregateRootId))
            {
                var format = "AggregateRootId cannot be null or empty if the command is not a ICreatingAggregateCommand, commandType:{0}, commandId:{1}.";
                throw new ArgumentException(string.Format(format, command.GetType().FullName, command.Id));
            }
            if (command is IProcessCommand && string.IsNullOrEmpty(((IProcessCommand)command).ProcessId))
            {
                var format = "ProcessId cannot be null or empty if the command is a IProcessCommand, commandType:{0}, commandId:{1}.";
                throw new ArgumentException(string.Format(format, command.GetType().FullName, command.Id));
            }
        }
        private Message BuildCommandMessage(ICommand command, string sourceEventId = null)
        {
            var raw = _binarySerializer.Serialize(command);
            var topic = _commandTopicProvider.GetTopic(command);
            var typeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            var commandData = ByteTypeDataUtils.Encode(new ByteTypeData(typeCode, raw));
            var messageData = _binarySerializer.Serialize(new CommandMessage
            {
                CommandData = commandData,
                CommandExecutedMessageTopic = _commandResultProcessor != null ? _commandResultProcessor.CommandExecutedMessageTopic : CommandExecutedMessageTopic,
                DomainEventHandledMessageTopic = _commandResultProcessor != null ? _commandResultProcessor.DomainEventHandledMessageTopic : DomainEventHandledMessageTopic,
                SourceEventId = sourceEventId
            });
            return new Message(topic, messageData);
        }
    }
}
