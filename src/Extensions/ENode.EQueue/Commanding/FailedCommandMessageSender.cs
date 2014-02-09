using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class FailedCommandMessageSender
    {
        private readonly Producer _producer;
        private readonly IBinarySerializer _binarySerializer;

        public Producer Producer { get { return _producer; } }

        public FailedCommandMessageSender() : this(new ProducerSetting()) { }
        public FailedCommandMessageSender(ProducerSetting setting) : this(null, setting) { }
        public FailedCommandMessageSender(string name, ProducerSetting setting) : this(setting, string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(FailedCommandMessageSender).Name : name, ObjectId.GenerateNewId())) { }
        public FailedCommandMessageSender(ProducerSetting setting, string id)
        {
            _producer = new Producer(setting, id);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        public FailedCommandMessageSender Start()
        {
            _producer.Start();
            return this;
        }
        public FailedCommandMessageSender Shutdown()
        {
            _producer.Shutdown();
            return this;
        }
        public void Send(FailedCommandMessage message, string topic)
        {
            _producer.SendAsync(new Message(topic, _binarySerializer.Serialize(message)), message.AggregateRootId);
        }
    }
}
