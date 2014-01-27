using System.Threading;
using System.Threading.Tasks;
using ECommon.IoC;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Socketing;
using ENode.Commanding;
using ENode.EQueue.Commanding;
using EQueue.Clients.Producers;
using EQueue.Protocols;
using EQueue.Utils;

namespace ENode.EQueue
{
    public class CommandService : ICommandService
    {
        private static int _commandServiceIndex;
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ICommandTopicProvider _commandTopicProvider;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly CompletedCommandProcessor _completedCommandProcessor;
        private readonly Producer _producer;

        public CommandService() : this(ProducerSetting.Default) { }
        public CommandService(ProducerSetting setting) : this(string.Format("{0}@{1}-{2}", SocketUtils.GetLocalIPV4(), typeof(CommandService).Name, Interlocked.Increment(ref _commandServiceIndex)), setting) { }
        public CommandService(string id, ProducerSetting setting)
        {
            _producer = new Producer(id, setting);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTopicProvider = ObjectContainer.Resolve<ICommandTopicProvider>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
            _completedCommandProcessor = ObjectContainer.Resolve<CompletedCommandProcessor>();
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
            var data = ByteTypeDataUtils.Encode(new ByteTypeData(typeCode, raw));
            var message = new Message(topic, data);
            var taskCompletionSource = new TaskCompletionSource<CommandResult>();

            _completedCommandProcessor.RegisterProcessingCommand(command, taskCompletionSource);
            _producer.SendAsync(message, command.AggregateRootId).ContinueWith(sendTask =>
            {
                if (sendTask.Result.SendStatus == SendStatus.Failed)
                {
                    _completedCommandProcessor.NotifyCommandSendFailed(command);
                }
            });
            return taskCompletionSource.Task;
        }
    }
}
