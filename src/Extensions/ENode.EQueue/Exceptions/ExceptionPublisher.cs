using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Exceptions;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class ExceptionPublisher : IMessagePublisher<IPublishableException>
    {
        private const string DefaultExceptionPublisherProcuderId = "ExceptionPublisher";
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ITopicProvider<IPublishableException> _exceptionTopicProvider;
        private readonly ITypeCodeProvider<IPublishableException> _exceptionTypeCodeProvider;
        private readonly Producer _producer;

        public Producer Producer { get { return _producer; } }

        public ExceptionPublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultExceptionPublisherProcuderId, setting ?? new ProducerSetting());
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _exceptionTopicProvider = ObjectContainer.Resolve<ITopicProvider<IPublishableException>>();
            _exceptionTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IPublishableException>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public ExceptionPublisher Start()
        {
            _producer.Start();
            return this;
        }
        public ExceptionPublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        public void Publish(IPublishableException exception)
        {
            var exceptionTypeCode = _exceptionTypeCodeProvider.GetTypeCode(exception.GetType());
            var topic = _exceptionTopicProvider.GetTopic(exception);
            var serializableInfo = new Dictionary<string, string>();
            exception.SerializeTo(serializableInfo);
            var exceptionMessage = new ExceptionMessage
            {
                UniqueId = exception.UniqueId,
                ExceptionTypeCode = exceptionTypeCode,
                SerializableInfo = serializableInfo
            };
            var data = _binarySerializer.Serialize(exceptionMessage);
            var message = new Message(topic, (int)MessageTypeCode.ExceptionMessage, data);
            var result = _producer.Send(message, exception.UniqueId);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new Exception(string.Format("Publish exception failed, exceptionId:{0}, exceptionType:{1}, exceptionData:{2}",
                    exception.UniqueId,
                    exception.GetType().Name,
                    string.Join("|", serializableInfo.Select(x => x.Key + ":" + x.Value))));
            }
        }
    }
}
