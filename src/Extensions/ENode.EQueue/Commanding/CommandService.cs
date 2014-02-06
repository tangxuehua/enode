using System.Threading.Tasks;
using ECommon.IoC;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.EQueue.Commanding;
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
        private readonly CommandResultProcessor _commandResultProcessor;
        private readonly Producer _producer;

        public CommandService(CommandResultProcessor commandResultProcessor) : this(commandResultProcessor, new ProducerSetting()) { }
        public CommandService(CommandResultProcessor commandResultProcessor, ProducerSetting setting) : this(commandResultProcessor, null, setting) { }
        public CommandService(CommandResultProcessor commandResultProcessor, string name, ProducerSetting setting) : this(commandResultProcessor, setting, string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(CommandService).Name : name, ObjectId.GenerateNewId())) { }
        public CommandService(CommandResultProcessor commandResultProcessor, ProducerSetting setting, string id)
        {
            _commandResultProcessor = commandResultProcessor;
            _producer = new Producer(setting, id);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTopicProvider = ObjectContainer.Resolve<ICommandTopicProvider>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
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
        public Task<CommandResult> Send(ICommand command)
        {
            var raw = _binarySerializer.Serialize(command);
            var topic = _commandTopicProvider.GetTopic(command);
            var typeCode = _commandTypeCodeProvider.GetTypeCode(command);
            var commandData = ByteTypeDataUtils.Encode(new ByteTypeData(typeCode, raw));
            var messageData = _binarySerializer.Serialize(new CommandMessage { CommandData = commandData, CommandResultTopic = _commandResultProcessor.CommandResultTopic });
            var taskCompletionSource = new TaskCompletionSource<CommandResult>();

            _commandResultProcessor.RegisterProcessingCommand(command, taskCompletionSource);
            _producer.SendAsync(new Message(topic, messageData), command.AggregateRootId).ContinueWith(sendTask =>
            {
                if (sendTask.Result.SendStatus == SendStatus.Failed)
                {
                    _commandResultProcessor.NotifyCommandSendFailed(command);
                }
            });
            return taskCompletionSource.Task;
        }
    }
}
