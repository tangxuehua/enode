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
        private static Task<AsyncTaskResult> _successResultTask = Task.FromResult(AsyncTaskResult.Success);

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

        public Task<AsyncTaskResult> PublishAsync(EventStream eventStream)
        {
            return _successResultTask;
        }
        public Task<AsyncTaskResult> PublishAsync(DomainEventStream eventStream)
        {
            return _successResultTask;
        }
        public Task<AsyncTaskResult> PublishAsync(IEvent evnt)
        {
            return _successResultTask;
        }
        public Task<AsyncTaskResult> PublishAsync(IMessage message)
        {
            return _successResultTask;
        }
        public Task<AsyncTaskResult> PublishAsync(IPublishableException exception)
        {
            return _successResultTask;
        }
    }
}
