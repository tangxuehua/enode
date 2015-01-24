using System;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.EQueue.Commanding;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class CommandService : ICommandService
    {
        private const string DefaultCommandExecutedMessageTopic = "CommandExecutedMessageTopic";
        private const string DefaultDomainEventHandledMessageTopic = "DomainEventHandledMessageTopic";
        private const string DefaultCommandServiceProcuderId = "CommandService";
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<ICommand> _commandTopicProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly ICommandRouteKeyProvider _commandRouteKeyProvider;
        private readonly CommandResultProcessor _commandResultProcessor;
        private readonly Producer _producer;
        private readonly IOHelper _ioHelper;

        public string CommandExecutedMessageTopic { get; private set; }
        public string DomainEventHandledMessageTopic { get; private set; }

        public CommandService(CommandResultProcessor commandResultProcessor = null, string id = null, ProducerSetting setting = null)
        {
            _commandResultProcessor = commandResultProcessor;
            _producer = new Producer(id ?? DefaultCommandServiceProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _commandTopicProvider = ObjectContainer.Resolve<ITopicProvider<ICommand>>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<ICommand>>();
            _commandRouteKeyProvider = ObjectContainer.Resolve<ICommandRouteKeyProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
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
            _ioHelper.TryIOAction(() =>
            {
                var result = _producer.Send(BuildCommandMessage(command), _commandRouteKeyProvider.GetRouteKey(command));
                if (result.SendStatus == SendStatus.Failed)
                {
                    throw new CommandSendException(result.ErrorMessage);
                }
            }, "SendCommand");
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

            _ioHelper.TryIOAction(() =>
            {
                var result = _producer.Send(BuildCommandMessage(command, sourceId, sourceType), _commandRouteKeyProvider.GetRouteKey(command));
                if (result.SendStatus == SendStatus.Failed)
                {
                    throw new CommandSendException(result.ErrorMessage);
                }
            }, "SendCommandFromSource");
        }
        public Task<CommandSendResult> SendAsync(ICommand command)
        {
            ValidateCommand(command);
            var message = BuildCommandMessage(command);
            var routeKey = _commandRouteKeyProvider.GetRouteKey(command);

            return _ioHelper.TryIOFunc<Task<CommandSendResult>>(() =>
            {
                return _producer.SendAsync(message, routeKey).ContinueWith<CommandSendResult>(sendTask =>
                {
                    return new CommandSendResult(
                            sendTask.Result.SendStatus == SendStatus.Success ? CommandSendStatus.Success : CommandSendStatus.Failed,
                            sendTask.Result.ErrorMessage);
                });
            }, "SendCommandAsync");
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

            var message = BuildCommandMessage(command);
            var routeKey = _commandRouteKeyProvider.GetRouteKey(command);

            _ioHelper.TryIOAction(() =>
            {
                _producer.SendAsync(message, routeKey).ContinueWith(sendTask =>
                {
                    if (sendTask.Result.SendStatus == SendStatus.Failed)
                    {
                        _commandResultProcessor.NotifyCommandSendFailed(command);
                    }
                });
            }, "ExecuteCommand");

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
            var commandData = _jsonSerializer.Serialize(command);
            var topic = _commandTopicProvider.GetTopic(command);
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            var commandExecutedMessageTopic = _commandResultProcessor != null ? _commandResultProcessor.CommandExecutedMessageTopic : CommandExecutedMessageTopic;
            var domainEventHandledMessageTopic = _commandResultProcessor != null ? _commandResultProcessor.DomainEventHandledMessageTopic : DomainEventHandledMessageTopic;
            var messageData = _jsonSerializer.Serialize(new CommandMessage
            {
                CommandTypeCode = commandTypeCode,
                CommandData = commandData,
                CommandExecutedMessageTopic = commandExecutedMessageTopic,
                DomainEventHandledMessageTopic = domainEventHandledMessageTopic,
                SourceId = sourceId,
                SourceType = sourceType
            });
            return new Message(topic, (int)EQueueMessageTypeCode.CommandMessage, Encoding.UTF8.GetBytes(messageData));
        }
    }
}
