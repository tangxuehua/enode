using System;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.EQueue.Commanding;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class CommandService : ICommandService
    {
        private const string DefaultCommandServiceProcuderId = "CommandService";
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<ICommand> _commandTopicProvider;
        private readonly ITypeCodeProvider _commandTypeCodeProvider;
        private readonly ICommandRoutingKeyProvider _commandRouteKeyProvider;
        private readonly SendQueueMessageService _sendMessageService;
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
            _commandTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider>();
            _commandRouteKeyProvider = ObjectContainer.Resolve<ICommandRoutingKeyProvider>();
            _sendMessageService = new SendQueueMessageService();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
        }

        public CommandService Start()
        {
            if (_commandResultProcessor != null)
            {
                _commandResultProcessor.Start();
            }
            _producer.Start();
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
            SendSync(command);
        }
        public void Send(ICommand command, string sourceId, string sourceType)
        {
            SendSync(command, sourceId, sourceType, true);
        }
        public Task<AsyncTaskResult> SendAsync(ICommand command)
        {
            try
            {
                var message = BuildCommandMessage(command);
                var routingKey = _commandRouteKeyProvider.GetRoutingKey(command);
                return _sendMessageService.SendMessageAsync(_producer, message, routingKey);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message));
            }
        }
        public Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command)
        {
            return ExecuteAsync(command, CommandReturnType.CommandExecuted);
        }
        public async Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command, CommandReturnType commandReturnType)
        {
            Ensure.NotNull(_commandResultProcessor, "commandResultProcessor");
            var taskCompletionSource = new TaskCompletionSource<AsyncTaskResult<CommandResult>>();
            _commandResultProcessor.RegisterProcessingCommand(command, commandReturnType, taskCompletionSource);

            try
            {
                var result = await SendAsync(command).ConfigureAwait(false);
                if (result.Status == AsyncTaskStatus.Success)
                {
                    return await taskCompletionSource.Task.ConfigureAwait(false);
                }
                _commandResultProcessor.ProcessFailedSendingCommand(command);
                return new AsyncTaskResult<CommandResult>(result.Status, result.ErrorMessage);
            }
            catch (Exception ex)
            {
                return new AsyncTaskResult<CommandResult>(AsyncTaskStatus.Failed, ex.Message);
            }
        }

        private void SendSync(ICommand command, string sourceId = null, string sourceType = null, bool checkSource = false)
        {
            if (checkSource)
            {
                Ensure.NotNullOrEmpty(sourceId, "sourceId");
                Ensure.NotNullOrEmpty(sourceType, "sourceType");
            }

            _ioHelper.TryIOAction(() =>
            {
                var result = _producer.Send(BuildCommandMessage(command, sourceId, sourceType), _commandRouteKeyProvider.GetRoutingKey(command));
                if (result.SendStatus == SendStatus.Failed)
                {
                    throw new CommandSendException(result.ErrorMessage);
                }
            }, "SendCommandSync");
        }
        private EQueueMessage BuildCommandMessage(ICommand command, string sourceId = null, string sourceType = null)
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
            return new EQueueMessage(topic, (int)EQueueMessageTypeCode.CommandMessage, Encoding.UTF8.GetBytes(messageData));
        }
    }
}
