using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Broker;
using EQueue.Configurations;

namespace ENode.PublishEventPerfTests
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static DomainEventPublisher _eventPublisher;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.RegisterEQueueComponents();
            _broker = new BrokerController();
            _eventPublisher = new DomainEventPublisher();
            configuration.SetDefault<IMessagePublisher<DomainEventStream>, DomainEventPublisher>(_eventPublisher);
            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _eventPublisher.Start();
            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _eventPublisher.Shutdown();
            _broker.Shutdown();
            return enodeConfiguration;
        }
    }
}
