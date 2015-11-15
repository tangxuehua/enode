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

        public void SendMessage(Producer producer, EQueueMessage message, string routingKey)
        {
            _ioHelper.TryIOAction(() =>
            {
                var result = producer.Send(message, routingKey);
                if (result.SendStatus != SendStatus.Success)
                {
                    throw new IOException(result.ErrorMessage);
                }
            }, "SendQueueMessage");
        }
        public async Task<AsyncTaskResult> SendMessageAsync(Producer producer, EQueueMessage message, string routingKey)
        {
            try
            {
                var result = await producer.SendAsync(message, routingKey);
                if (result.SendStatus != SendStatus.Success)
                {
                    _logger.ErrorFormat("EQueue message send failed, sendResult: {0}, routingKey: {1}", result, routingKey);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, result.ErrorMessage);
                }
                _logger.InfoFormat("EQueue message send success, sendResult: {0}, routingKey: {1}", result, routingKey);
                return AsyncTaskResult.Success;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("EQueue message send has exception, message: {0}, routingKey: {1}", message, routingKey);
                return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
            }
        }
    }
}
