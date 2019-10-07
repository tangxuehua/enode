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

        public async Task<AsyncTaskResult> SendMessageAsync(Producer producer, string messageType, string messageClass, EQueueMessage message, string routingKey, string messageId, IDictionary<string, string> messageExtensionItems)
        {
            try
            {
                var result = await producer.SendAsync(message, routingKey).ConfigureAwait(false);
                if (result.SendStatus != SendStatus.Success)
                {
                    _logger.ErrorFormat("ENode {0} message send failed, message: {1}, sendResult: {2}, routingKey: {3}, messageType: {4}, messageId: {5}, messageExtensionItems: {6}", 
                        messageType,
                        message,
                        result, 
                        routingKey,
                        messageClass,
                        messageId, 
                        _jsonSerializer.Serialize(messageExtensionItems)
                    );
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, result.ErrorMessage);
                }
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("ENode {0} message send success, equeueMessageId: {1}, routingKey: {2}, messageType: {3}, messageId: {4}, messageExtensionItems: {5}",
                        messageType,
                        result.MessageStoreResult.MessageId,
                        routingKey,
                        messageClass,
                        messageId,
                        _jsonSerializer.Serialize(messageExtensionItems)
                    );
                }
                return AsyncTaskResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("ENode {0} message send has exception, message: {1}, routingKey: {2}, messageType: {3}, messageId: {4}, messageExtensionItems: {5}", 
                    messageType, 
                    message, 
                    routingKey,
                    messageClass,
                    messageId, 
                    _jsonSerializer.Serialize(messageExtensionItems)
                ), ex);
                return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
            }
        }
    }
}
