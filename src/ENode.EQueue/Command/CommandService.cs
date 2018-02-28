using System;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Extensions;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class CommandService : ICommandService
    {
        private ILogger _logger;
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<ICommand> _commandTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private ICommandRoutingKeyProvider _commandRouteKeyProvider;
        private SendQueueMessageService _sendMessageService;
        private CommandResultProcessor _commandResultProcessor;
        private Producer _producer;
        private IOHelper _ioHelper;

        public string CommandExecutedMessageTopic { get; private set; }
        public string DomainEventHandledMessageTopic { get; private set; }

        public CommandService InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _commandTopicProvider = ObjectContainer.Resolve<ITopicProvider<ICommand>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _commandRouteKeyProvider = ObjectContainer.Resolve<ICommandRoutingKeyProvider>();
            _sendMessageService = new SendQueueMessageService();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            return this;
        }
        public CommandService InitializeEQueue(CommandResultProcessor commandResultProcessor = null, ProducerSetting setting = null)
        {
            InitializeENode();
            _commandResultProcessor = commandResultProcessor;
            _producer = new Producer(setting);
            return this;
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
            _sendMessageService.SendMessage(_producer, BuildCommandMessage(command, false), _commandRouteKeyProvider.GetRoutingKey(command), command.Id, null);
        }
        public Task<AsyncTaskResult> SendAsync(ICommand command)
        {
            try
            {
                return _sendMessageService.SendMessageAsync(_producer, BuildCommandMessage(command, false), _commandRouteKeyProvider.GetRoutingKey(command), command.Id, null);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message));
            }
        }
        public CommandResult Execute(ICommand command, int timeoutMillis)
        {
            var result = ExecuteAsync(command).WaitResult(timeoutMillis);
            if (result == null)
            {
                throw new CommandExecuteTimeoutException("Command execute timeout, commandId: {0}, aggregateRootId: {1}", command.Id, command.AggregateRootId);
            }
            return result.Data;
        }
        public CommandResult Execute(ICommand command, CommandReturnType commandReturnType, int timeoutMillis)
        {
            var result = ExecuteAsync(command, commandReturnType).WaitResult(timeoutMillis);
            if (result == null)
            {
                throw new CommandExecuteTimeoutException("Command execute timeout, commandId: {0}, aggregateRootId: {1}", command.Id, command.AggregateRootId);
            }
            return result.Data;
        }
        public Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command)
        {
            return ExecuteAsync(command, CommandReturnType.CommandExecuted);
        }
        public async Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command, CommandReturnType commandReturnType)
        {
            try
            {
                Ensure.NotNull(_commandResultProcessor, "commandResultProcessor");
                var taskCompletionSource = new TaskCompletionSource<AsyncTaskResult<CommandResult>>();
                _commandResultProcessor.RegisterProcessingCommand(command, commandReturnType, taskCompletionSource);

                var result = await _sendMessageService.SendMessageAsync(_producer, BuildCommandMessage(command, true), _commandRouteKeyProvider.GetRoutingKey(command), command.Id, null).ConfigureAwait(false);
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

        private EQueueMessage BuildCommandMessage(ICommand command, bool needReply = false)
        {
            Ensure.NotNull(command.AggregateRootId, "aggregateRootId");
            var commandData = _jsonSerializer.Serialize(command);
            var topic = _commandTopicProvider.GetTopic(command);
            var replyAddress = needReply && _commandResultProcessor != null ? _commandResultProcessor.BindingAddress.ToString() : null;
            var messageData = _jsonSerializer.Serialize(new CommandMessage
            {
                CommandData = commandData,
                ReplyAddress = replyAddress
            });
            return new EQueueMessage(
                topic, 
                (int)EQueueMessageTypeCode.CommandMessage,
                Encoding.UTF8.GetBytes(messageData),
                _typeNameProvider.GetTypeName(command.GetType()));
        }
    }
}
