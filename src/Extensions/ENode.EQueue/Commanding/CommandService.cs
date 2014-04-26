using System.Threading.Tasks;
using ECommon.IoC;
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
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ICommandTopicProvider _commandTopicProvider;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly ICommandRouteKeyProvider _commandRouteKeyProvider;
        private readonly CommandResultProcessor _commandResultProcessor;
        private readonly Producer _producer;

        public CommandService(CommandResultProcessor commandResultProcessor) : this(commandResultProcessor, "CommandService") { }
        public CommandService(CommandResultProcessor commandResultProcessor, string id) : this(commandResultProcessor, id, new ProducerSetting()) { }
        public CommandService(CommandResultProcessor commandResultProcessor, string id, ProducerSetting setting)
        {
            _commandResultProcessor = commandResultProcessor;
            _producer = new Producer(id, setting);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTopicProvider = ObjectContainer.Resolve<ICommandTopicProvider>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
            _commandRouteKeyProvider = ObjectContainer.Resolve<ICommandRouteKeyProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
        }

        public CommandService Start()
        {
            _producer.Start();
            return this;
        }
        public CommandService Shutdown()
        {
            _producer.Shutdown();
            return this;
        }
        public void Send(ICommand command)
        {
            VerifyCommand(command);
            _producer.Send(BuildCommandMessage(command), _commandRouteKeyProvider.GetRouteKey(command));
        }
        public Task<CommandResult> Execute(ICommand command)
        {
            return Execute(command, CommandReturnType.DomainEventHandled);
        }
        public Task<CommandResult> Execute(ICommand command, CommandReturnType commandReturnType)
        {
            VerifyCommand(command);
            var message = BuildCommandMessage(command);
            var taskCompletionSource = new TaskCompletionSource<CommandResult>();

            _commandResultProcessor.RegisterCommand(command, commandReturnType, taskCompletionSource);

            _producer.SendAsync(message, _commandRouteKeyProvider.GetRouteKey(command)).ContinueWith(sendTask =>
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
            VerifyCommand(command);
            var message = BuildCommandMessage(command);
            var taskCompletionSource = new TaskCompletionSource<ProcessResult>();

            _commandResultProcessor.RegisterProcess(command, taskCompletionSource);

            _producer.SendAsync(message, _commandRouteKeyProvider.GetRouteKey(command)).ContinueWith(sendTask =>
            {
                if (sendTask.Result.SendStatus == SendStatus.Failed)
                {
                    _commandResultProcessor.NotifyProcessCommandSendFailed(command);
                }
            });

            return taskCompletionSource.Task;
        }

        private void VerifyCommand(ICommand command)
        {
            if (!(command is ICreatingAggregateCommand) && string.IsNullOrEmpty(command.AggregateRootId))
            {
                var format = "AggregateRootId cannot be null or empty if the command is not a CreatingAggregateCommand, commandType:{0}, commandId:{1}.";
                throw new ENodeException(format, command.GetType().FullName, command.Id);
            }
        }
        private Message BuildCommandMessage(ICommand command)
        {
            var raw = _binarySerializer.Serialize(command);
            var topic = _commandTopicProvider.GetTopic(command);
            var typeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            var commandData = ByteTypeDataUtils.Encode(new ByteTypeData(typeCode, raw));
            var messageData = _binarySerializer.Serialize(new CommandMessage
            {
                CommandData = commandData,
                CommandExecutedMessageTopic = _commandResultProcessor.CommandExecutedMessageTopic,
                DomainEventHandledMessageTopic = _commandResultProcessor.DomainEventHandledMessageTopic
            });
            return new Message(topic, messageData);
        }
    }
}
