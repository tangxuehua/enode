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

        public Task<PublishResult<EventStream>> PublishAsync(EventStream eventStream)
        {
            var taskCompletionSource = new TaskCompletionSource<PublishResult<EventStream>>();
            taskCompletionSource.SetResult(new PublishResult<EventStream>(PublishStatus.Success, null, eventStream));
            return taskCompletionSource.Task;
        }
        public Task<PublishResult<DomainEventStream>> PublishAsync(DomainEventStream eventStream)
        {
            var taskCompletionSource = new TaskCompletionSource<PublishResult<DomainEventStream>>();
            taskCompletionSource.SetResult(new PublishResult<DomainEventStream>(PublishStatus.Success, null, eventStream));
            return taskCompletionSource.Task;
        }
        public Task<PublishResult<IEvent>> PublishAsync(IEvent evnt)
        {
            var taskCompletionSource = new TaskCompletionSource<PublishResult<IEvent>>();
            taskCompletionSource.SetResult(new PublishResult<IEvent>(PublishStatus.Success, null, evnt));
            return taskCompletionSource.Task;
        }
        public Task<PublishResult<IMessage>> PublishAsync(IMessage message)
        {
            var taskCompletionSource = new TaskCompletionSource<PublishResult<IMessage>>();
            taskCompletionSource.SetResult(new PublishResult<IMessage>(PublishStatus.Success, null, message));
            return taskCompletionSource.Task;
        }
        public Task<PublishResult<IPublishableException>> PublishAsync(IPublishableException exception)
        {
            var taskCompletionSource = new TaskCompletionSource<PublishResult<IPublishableException>>();
            taskCompletionSource.SetResult(new PublishResult<IPublishableException>(PublishStatus.Success, null, exception));
            return taskCompletionSource.Task;
        }
    }
}
