using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Eventing;
using ENode.Exceptions;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class ExceptionConsumer : IQueueMessageHandler
    {
        private const string DefaultExceptionConsumerId = "ExceptionConsumer";
        private const string DefaultExceptionConsumerGroup = "ExceptionConsumerGroup";
        private readonly Consumer _consumer;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ITypeCodeProvider<IPublishableException> _exceptionTypeCodeProvider;
        private readonly IMessageProcessor<IPublishableException> _exceptionProcessor;
        private readonly ILogger _logger;

        public Consumer Consumer { get { return _consumer; } }

        public ExceptionConsumer(string id = null, string groupName = null, ConsumerSetting setting = null, DomainEventHandledMessageSender domainEventHandledMessageSender = null, bool sendEventHandledMessage = true)
        {
            var consumerId = id ?? DefaultExceptionConsumerId;
            _consumer = new Consumer(consumerId, groupName ?? DefaultExceptionConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential
            });
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _exceptionProcessor = ObjectContainer.Resolve<IMessageProcessor<IPublishableException>>();
            _exceptionTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IPublishableException>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public ExceptionConsumer Start()
        {
            _consumer.SetMessageHandler(this).Start();
            return this;
        }
        public ExceptionConsumer Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            return this;
        }
        public ExceptionConsumer Shutdown()
        {
            _consumer.Shutdown();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var exceptionMessage = _binarySerializer.Deserialize<ExceptionMessage>(message.Body);
            var exceptionType = _exceptionTypeCodeProvider.GetType(exceptionMessage.ExceptionTypeCode);
            var exception = FormatterServices.GetUninitializedObject(exceptionType) as IPublishableException;
            exception.UniqueId = exceptionMessage.UniqueId;
            exception.RestoreFrom(exceptionMessage.SerializableInfo);
            _exceptionProcessor.Process(exception, new ExceptionProcessContext(message, context, EventProcessedCallback));
        }

        private void EventProcessedCallback(IPublishableException exception, ExceptionProcessContext exceptionProcessContext)
        {
            exceptionProcessContext.MessageContext.OnMessageHandled(exceptionProcessContext.QueueMessage);
        }

        class ExceptionProcessContext : IMessageProcessContext<IPublishableException>
        {
            public Action<IPublishableException, ExceptionProcessContext> ExceptionProcessedAction { get; private set; }
            public IMessageContext MessageContext { get; private set; }
            public QueueMessage QueueMessage { get; private set; }

            public ExceptionProcessContext(QueueMessage queueMessage, IMessageContext messageContext, Action<IPublishableException, ExceptionProcessContext> exceptionProcessedAction)
            {
                QueueMessage = queueMessage;
                MessageContext = messageContext;
                ExceptionProcessedAction = exceptionProcessedAction;
            }

            public void OnMessageProcessed(IPublishableException exception)
            {
                ExceptionProcessedAction(exception, this);
            }
        }
    }
}
