using System;
using System.Threading.Tasks;
using ECommon.Components;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    internal class SendQueueMessageService
    {
        private readonly IOHelper _ioHelper;

        public SendQueueMessageService()
        {
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
        }

        public void SendMessage(Producer producer, Message message, object routingKey)
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
        public async Task<AsyncTaskResult> SendMessageAsync(Producer producer, Message message, object routingKey)
        {
            try
            {
                var result = await producer.SendAsync(message, routingKey);
                if (result.SendStatus != SendStatus.Success)
                {
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, result.ErrorMessage);
                }
                return AsyncTaskResult.Success;
            }
            catch (Exception ex)
            {
                return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
            }
        }
    }
}
