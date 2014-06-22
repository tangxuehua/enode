using ECommon.Components;
using ECommon.Serializing;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class CommandExecutedMessageSender
    {
        private const string DefaultCommandExecutedMessageSenderProcuderId = "sys_cemsp";
        private readonly Producer _producer;
        private readonly IBinarySerializer _binarySerializer;

        public Producer Producer { get { return _producer; } }

        public CommandExecutedMessageSender() : this(DefaultCommandExecutedMessageSenderProcuderId) { }
        public CommandExecutedMessageSender(string id) : this(id, new ProducerSetting()) { }
        public CommandExecutedMessageSender(string id, ProducerSetting setting)
        {
            _producer = new Producer(id, setting);
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
            _producer.SendAsync(new Message(topic, _binarySerializer.Serialize(message)), message.CommandId);
        }
    }
}
