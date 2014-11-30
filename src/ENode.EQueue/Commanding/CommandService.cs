using System;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.EQueue.Commanding;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;
using EQueue.Utils;

namespace ENode.EQueue
{
    public class CommandService : ICommandService
    {
        private const string DefaultCommandExecutedMessageTopic = "sys_ecmt";
        private const string DefaultDomainEventHandledMessageTopic = "sys_dehmt";
        private const string DefaultCommandServiceProcuderId = "CommandService";
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ITopicProvider<ICommand> _commandTopicProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
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
            _commandTopicProvider = ObjectContainer.Resolve<ITopicProvider<ICommand>>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<ICommand>>();
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
        public void Send(ICommand command, string sourceId, string sourceType)
        {
            ValidateCommand(command);
            if (string.IsNullOrEmpty(sourceId))
            {
                throw new ArgumentNullException("sourceId.");
            }
            if (string.IsNullOrEmpty(sourceType))
            {
                throw new ArgumentNullException("sourceType.");
            }
            var result = _producer.Send(BuildCommandMessage(command, sourceId, sourceType), _commandRouteKeyProvider.GetRouteKey(command));
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
            return Execute(command, CommandReturnType.CommandExecuted);
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

        private void ValidateCommand(ICommand command)
        {
            if (string.IsNullOrEmpty(command.Id))
            {
                throw new ArgumentException("Command id can not be null or empty.");
            }
            if (!(command is ICreatingAggregateCommand) && command is IAggregateCommand && string.IsNullOrEmpty(((IAggregateCommand)command).AggregateRootId))
            {
                var format = "AggregateRootId cannot be null or empty if the aggregate command is not a ICreatingAggregateCommand, commandType:{0}, commandId:{1}.";
                throw new ArgumentException(string.Format(format, command.GetType().FullName, command.Id));
            }
        }
        private Message BuildCommandMessage(ICommand command, string sourceId = null, string sourceType = null)
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
                SourceId = sourceId,
                SourceType = sourceType
            });
            return new Message(topic, (int)EQueueMessageTypeCode.CommandMessage, messageData);
        }
    }
}
