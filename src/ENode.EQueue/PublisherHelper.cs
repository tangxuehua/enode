using System.Threading.Tasks;
using ECommon.Components;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    internal class PublisherHelper
    {
        private readonly IOHelper _ioHelper;

        public PublisherHelper()
        {
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
        }

        public void PublishQueueMessage(Producer producer, Message queueMessage, object routingKey, string actionName)
        {
            _ioHelper.TryIOAction(() =>
            {
                var result = producer.Send(queueMessage, routingKey);
                if (result.SendStatus != SendStatus.Success)
                {
                    throw new IOException(result.ErrorMessage);
                }
            }, actionName);
        }
        public Task<AsyncOperationResult> PublishQueueMessageAsync(Producer producer, Message queueMessage, object routingKey, string actionName)
        {
            return _ioHelper.TryIOFuncAsync(() =>
            {
                return producer.SendAsync(queueMessage, routingKey).ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        return new AsyncOperationResult(AsyncOperationResultStatus.IOException, t.Exception.InnerException.Message);
                    }
                    var result = t.Result;
                    if (result.SendStatus != SendStatus.Success)
                    {
                        return new AsyncOperationResult(AsyncOperationResultStatus.IOException, result.ErrorMessage);
                    }
                    return AsyncOperationResult.Success;
                });
            }, actionName);
        }
    }
}
