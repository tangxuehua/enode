using System.Threading.Tasks;
using ENode.Eventing;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace ENode.Messaging.Impl
{
    public class DoNothingPublisher :
        IPublisher<EventStream>,
        IPublisher<DomainEventStream>,
        IPublisher<IEvent>,
        IPublisher<IMessage>,
        IPublisher<IPublishableException>
    {
        public void Publish(IEvent evnt)
        {
        }
        public void Publish(DomainEventStream eventStream)
        {
        }
        public void Publish(EventStream eventStream)
        {
        }
        public void Publish(IMessage message)
        {
        }
        public void Publish(IPublishableException exception)
        {
        }

        public Task<AsyncOperationResult> PublishAsync(EventStream eventStream)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult>();
            taskCompletionSource.SetResult(AsyncOperationResult.Success);
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult> PublishAsync(DomainEventStream eventStream)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult>();
            taskCompletionSource.SetResult(AsyncOperationResult.Success);
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult> PublishAsync(IEvent evnt)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult>();
            taskCompletionSource.SetResult(AsyncOperationResult.Success);
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult> PublishAsync(IMessage message)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult>();
            taskCompletionSource.SetResult(AsyncOperationResult.Success);
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult> PublishAsync(IPublishableException exception)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult>();
            taskCompletionSource.SetResult(AsyncOperationResult.Success);
            return taskCompletionSource.Task;
        }
    }
}
