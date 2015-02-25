using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Serializing;
using ENode.Exceptions;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class PublishableExceptionPublisher : IPublisher<IPublishableException>
    {
        private const string DefaultExceptionPublisherProcuderId = "ExceptionPublisher";
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<IPublishableException> _exceptionTopicProvider;
        private readonly ITypeCodeProvider<IPublishableException> _exceptionTypeCodeProvider;
        private readonly Producer _producer;
        private readonly PublisherHelper _publisherHelper;

        public Producer Producer { get { return _producer; } }

        public PublishableExceptionPublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultExceptionPublisherProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _exceptionTopicProvider = ObjectContainer.Resolve<ITopicProvider<IPublishableException>>();
            _exceptionTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IPublishableException>>();
            _publisherHelper = new PublisherHelper();
        }

        public PublishableExceptionPublisher Start()
        {
            _producer.Start();
            return this;
        }
        public PublishableExceptionPublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        public void Publish(IPublishableException exception)
        {
            var message = CreateEQueueMessage(exception);
            _publisherHelper.PublishQueueMessage(_producer, message, exception.Id, "Send exception message to broker");
        }
        public Task<AsyncOperationResult> PublishAsync(IPublishableException exception)
        {
            var message = CreateEQueueMessage(exception);
            return _publisherHelper.PublishQueueMessageAsync(_producer, message, exception.Id, "Send exception message to broker");
        }

        private Message CreateEQueueMessage(IPublishableException exception)
        {
            var exceptionTypeCode = _exceptionTypeCodeProvider.GetTypeCode(exception.GetType());
            var topic = _exceptionTopicProvider.GetTopic(exception);
            var serializableInfo = new Dictionary<string, string>();
            exception.SerializeTo(serializableInfo);
            var data = _jsonSerializer.Serialize(new PublishableExceptionMessage
            {
                UniqueId = exception.Id,
                ExceptionTypeCode = exceptionTypeCode,
                SerializableInfo = serializableInfo
            });
            return new Message(topic, (int)EQueueMessageTypeCode.ExceptionMessage, Encoding.UTF8.GetBytes(data));
        }
    }
}
