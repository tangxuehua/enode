using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class DomainExceptionPublisher : IMessagePublisher<IDomainException>
    {
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IDomainException> _exceptionTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private SendQueueMessageService _sendMessageService;

        public Producer Producer { get; private set; }

        public DomainExceptionPublisher InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _exceptionTopicProvider = ObjectContainer.Resolve<ITopicProvider<IDomainException>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _sendMessageService = new SendQueueMessageService();
            return this;
        }
        public DomainExceptionPublisher InitializeEQueue(ProducerSetting setting = null)
        {
            InitializeENode();
            Producer = new Producer(setting, "DomainExceptionPublisher");
            return this;
        }
        public DomainExceptionPublisher Start()
        {
            Producer.Start();
            return this;
        }
        public DomainExceptionPublisher Shutdown()
        {
            Producer.Shutdown();
            return this;
        }
        public Task PublishAsync(IDomainException exception)
        {
            var topic = _exceptionTopicProvider.GetTopic(exception);
            var serializableInfo = new Dictionary<string, string>();
            exception.SerializeTo(serializableInfo);
            var data = _jsonSerializer.Serialize(new DomainExceptionMessage
            {
                UniqueId = exception.Id,
                Timestamp = exception.Timestamp,
                Items = exception.Items,
                SerializableInfo = serializableInfo
            });
            var equeueMessage = new EQueueMessage(
                topic,
                (int)EQueueMessageTypeCode.ExceptionMessage,
                Encoding.UTF8.GetBytes(data),
                _typeNameProvider.GetTypeName(exception.GetType()));

            return _sendMessageService.SendMessageAsync(Producer, "exception", exception.GetType().Name, equeueMessage, data, exception.Id, exception.Id, exception.Items);
        }
    }
}
