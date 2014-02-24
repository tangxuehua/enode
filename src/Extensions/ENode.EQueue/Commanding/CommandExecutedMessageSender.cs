using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class CommandExecutedMessageSender
    {
        private readonly Producer _producer;
        private readonly IBinarySerializer _binarySerializer;

        public Producer Producer { get { return _producer; } }

        public CommandExecutedMessageSender() : this(new ProducerSetting()) { }
        public CommandExecutedMessageSender(ProducerSetting setting) : this(null, setting) { }
        public CommandExecutedMessageSender(string name, ProducerSetting setting) : this(setting, string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(CommandExecutedMessageSender).Name : name, ObjectId.GenerateNewId())) { }
        public CommandExecutedMessageSender(ProducerSetting setting, string id)
        {
            _producer = new Producer(setting, id);
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
            _producer.SendAsync(new Message(topic, _binarySerializer.Serialize(message)), message.AggregateRootId);
        }
    }
}
