using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    internal class SendQueueMessageService
    {
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public SendQueueMessageService()
        {
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
        }

        public async Task SendMessageAsync(Producer producer, string messageType, string messageClass, EQueueMessage message, string messageBodyString, string routingKey, string messageId, IDictionary<string, string> messageExtensionItems)
        {
            try
            {
                var result = await producer.SendAsync(message, routingKey).ConfigureAwait(false);
                if (result.SendStatus == SendStatus.Success)
                {
                    _logger.InfoFormat("ENode {0} message send success, equeueMessageId: {1}, message: {2}, messageBody: {3}, routingKey: {4}, messageType: {5}, messageId: {6}, messageExtensionItems: {7}",
                        messageType,
                        result.MessageStoreResult.MessageId,
                        message,
                        messageBodyString,
                        routingKey,
                        messageClass,
                        messageId,
                        _jsonSerializer.Serialize(messageExtensionItems)
                    );
                }
                else
                {
                    _logger.ErrorFormat("ENode {0} message send failed, message: {1}, messageBody: {2}, sendResult: {3}, routingKey: {4}, messageType: {5}, messageId: {6}, messageExtensionItems: {7}",
                        messageType,
                        message,
                        messageBodyString,
                        result,
                        routingKey,
                        messageClass,
                        messageId,
                        _jsonSerializer.Serialize(messageExtensionItems)
                    );
                    throw new IOException(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("ENode {0} message send has exception, message: {1}, messageBody: {2}, routingKey: {3}, messageType: {4}, messageId: {5}, messageExtensionItems: {6}", 
                    messageType, 
                    message,
                    messageBodyString,
                    routingKey,
                    messageClass,
                    messageId, 
                    _jsonSerializer.Serialize(messageExtensionItems)
                ), ex);
                throw new IOException("Send equeue message has exception.", ex);
            }
        }
    }
}
