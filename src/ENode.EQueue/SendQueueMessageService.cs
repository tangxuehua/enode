using System;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    internal class SendQueueMessageService
    {
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        public SendQueueMessageService()
        {
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public async Task<AsyncTaskResult> SendMessageAsync(Producer producer, EQueueMessage message, string routingKey, string messageId, string version)
        {
            try
            {
                var result = await producer.SendAsync(message, routingKey);
                if (result.SendStatus != SendStatus.Success)
                {
                    _logger.ErrorFormat("ENode message async send failed, sendResult: {0}, routingKey: {1}, messageId: {2}, version: {3}", result, routingKey, messageId, version);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, result.ErrorMessage);
                }
                _logger.DebugFormat("ENode message async send success, sendResult: {0}, routingKey: {1}, messageId: {2}, version: {3}", result, routingKey, messageId, version);
                return AsyncTaskResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("ENode message async send has exception, message: {0}, routingKey: {1}, messageId: {2}, version: {3}", message, routingKey, messageId, version), ex);
                return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
            }
        }
    }
}
