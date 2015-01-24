using System.Text;
using ECommon.Components;
using ECommon.Serializing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class CommandExecutedMessageSender
    {
        private const string DefaultCommandExecutedMessageSenderProcuderId = "CommandExecutedMessageSender";
        private readonly Producer _producer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IOHelper _ioHelper;

        public Producer Producer { get { return _producer; } }

        public CommandExecutedMessageSender(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultCommandExecutedMessageSenderProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
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
            var messageJson = _jsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var equeueMessage = new Message(topic, (int)EQueueMessageTypeCode.CommandExecutedMessage, messageBytes);
            _ioHelper.TryIOActionRecursively("SendCommandExecutedMessage", () => messageJson, () =>
            {
                _ioHelper.TryIOAction(() => _producer.SendAsync(equeueMessage, message.CommandId), "SendCommandExecutedMessageAsync");
            });
        }
    }
}
