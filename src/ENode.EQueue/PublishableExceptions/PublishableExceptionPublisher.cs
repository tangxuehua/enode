using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Serializing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class PublishableExceptionPublisher : IMessagePublisher<IPublishableException>
    {
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IPublishableException> _exceptionTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private SendQueueMessageService _sendMessageService;

        public Producer Producer { get; private set; }

        public PublishableExceptionPublisher InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _exceptionTopicProvider = ObjectContainer.Resolve<ITopicProvider<IPublishableException>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _sendMessageService = new SendQueueMessageService();
            return this;
        }
        public PublishableExceptionPublisher InitializeEQueue(ProducerSetting setting = null)
        {
            InitializeENode();
            Producer = new Producer(setting, "PublishableExceptionPublisher");
            return this;
        }
        public PublishableExceptionPublisher Start()
        {
            Producer.Start();
            return this;
        }
        public PublishableExceptionPublisher Shutdown()
        {
            Producer.Shutdown();
            return this;
        }
        public Task<AsyncTaskResult> PublishAsync(IPublishableException exception)
        {
            var message = CreateEQueueMessage(exception);
            return _sendMessageService.SendMessageAsync(Producer, message, exception.Id, exception.Id, null);
        }

        private EQueueMessage CreateEQueueMessage(IPublishableException exception)
        {
            var topic = _exceptionTopicProvider.GetTopic(exception);
            var serializableInfo = new Dictionary<string, string>();
            exception.SerializeTo(serializableInfo);
            var data = _jsonSerializer.Serialize(new PublishableExceptionMessage
            {
                UniqueId = exception.Id,
                Timestamp = exception.Timestamp,
                SerializableInfo = serializableInfo
            });
            return new EQueueMessage(
                topic,
                (int)EQueueMessageTypeCode.ExceptionMessage,
                Encoding.UTF8.GetBytes(data),
                _typeNameProvider.GetTypeName(exception.GetType()));
        }
    }
}
