using System.Threading.Tasks;
using ECommon.IO;
using ENode.Eventing;

namespace ENode.Infrastructure.Impl
{
    public class DoNothingPublisher :
        IMessagePublisher<DomainEventStreamMessage>,
        IMessagePublisher<IApplicationMessage>,
        IMessagePublisher<IPublishableException>
    {
        private static Task<AsyncTaskResult> _successResultTask = Task.FromResult(AsyncTaskResult.Success);

        public Task<AsyncTaskResult> PublishAsync(DomainEventStreamMessage message)
        {
            return _successResultTask;
        }
        public Task<AsyncTaskResult> PublishAsync(IApplicationMessage message)
        {
            return _successResultTask;
        }
        public Task<AsyncTaskResult> PublishAsync(IPublishableException exception)
        {
            return _successResultTask;
        }
    }
}
