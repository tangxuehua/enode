using System;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    internal class SendQueueMessageService
    {
        private readonly IOHelper _ioHelper;

        public SendQueueMessageService()
        {
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
        }

        public void SendMessage(Producer producer, EQueueMessage message, object routingKey)
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
        public async Task<AsyncTaskResult> SendMessageAsync(Producer producer, EQueueMessage message, object routingKey)
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
