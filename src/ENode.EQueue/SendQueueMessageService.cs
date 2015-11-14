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
            try
            {
                _ioHelper.TryIOAction(() =>
                {
                    var result = producer.Send(message, routingKey);
                    if (result.SendStatus != SendStatus.Success)
                    {
                        _logger.ErrorFormat("EQueue message synch send failed, sendResult: {0}, routingKey: {1}", result, routingKey);
                        throw new IOException(result.ErrorMessage);
                    }
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("EQueue message synch send success, sendResult: {0}, routingKey: {1}", result, routingKey);
                    }
                }, "SendQueueMessage");
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("EQueue message synch send has exception, message: {0}, routingKey: {1}", message, routingKey), ex);
                throw;
            }
        }
        public async Task<AsyncTaskResult> SendMessageAsync(Producer producer, EQueueMessage message, string routingKey)
        {
            try
            {
                var result = await producer.SendAsync(message, routingKey);
                if (result.SendStatus != SendStatus.Success)
                {
                    _logger.ErrorFormat("EQueue message async send failed, sendResult: {0}, routingKey: {1}", result, routingKey);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, result.ErrorMessage);
                }
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("EQueue message async send success, sendResult: {0}, routingKey: {1}", result, routingKey);
                }
                return AsyncTaskResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("EQueue message async send has exception, message: {0}, routingKey: {1}", message, routingKey), ex);
                return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
            }
        }
    }
}
