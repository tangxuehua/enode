using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using ENode.Commanding;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class CommandResultSender
    {
        private readonly Producer _producer;
        private readonly IBinarySerializer _binarySerializer;

        public Producer Producer { get { return _producer; } }

        public CommandResultSender() : this(new ProducerSetting()) { }
        public CommandResultSender(ProducerSetting setting) : this(null, setting) { }
        public CommandResultSender(string name, ProducerSetting setting) : this(setting, string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(CommandResultSender).Name : name, ObjectId.GenerateNewId())) { }
        public CommandResultSender(ProducerSetting setting, string id)
        {
            _producer = new Producer(setting, id);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        public CommandResultSender Start()
        {
            _producer.Start();
            return this;
        }
        public CommandResultSender Shutdown()
        {
            _producer.Shutdown();
            return this;
        }
        public void Send(CommandResult commandResult, string topic)
        {
            _producer.SendAsync(new Message(topic, _binarySerializer.Serialize(commandResult)), commandResult.AggregateRootId);
        }
    }
}
