using ECommon.Components;
using ECommon.Serializing;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class CommandExecutedMessageSender
    {
        private const string DefaultCommandExecutedMessageSenderProcuderId = "CommandExecutedMessageSender";
        private readonly Producer _producer;
        private readonly IBinarySerializer _binarySerializer;

        public Producer Producer { get { return _producer; } }

        public CommandExecutedMessageSender(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultCommandExecutedMessageSenderProcuderId, setting ?? new ProducerSetting());
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        public CommandExecutedMessageSender Start()
        {
            _producer.Start();
            return this;
        }
        public CommandExecutedMessageSender Shutdown()
        {
            _producer.Shutdown();
            return this;
        }
        public void Send(CommandExecutedMessage message, string topic)
        {
            _producer.SendAsync(new Message(topic, (int)EQueueMessageTypeCode.CommandExecutedMessage, _binarySerializer.Serialize(message)), message.CommandId);
        }
    }
}
