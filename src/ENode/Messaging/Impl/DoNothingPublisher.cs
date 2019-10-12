using System.Threading.Tasks;
using ENode.Domain;
using ENode.Eventing;

namespace ENode.Messaging.Impl
{
    public class DoNothingPublisher :
        IMessagePublisher<DomainEventStreamMessage>,
        IMessagePublisher<IApplicationMessage>,
        IMessagePublisher<IDomainException>
    {
        public Task PublishAsync(DomainEventStreamMessage message)
        {
            return Task.CompletedTask;
        }
        public Task PublishAsync(IApplicationMessage message)
        {
            return Task.CompletedTask;
        }
        public Task PublishAsync(IDomainException exception)
        {
            return Task.CompletedTask;
        }
    }
}
