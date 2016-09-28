using System.IO;
using ENode.Configurations;
using ENode.EQueue;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Broker;
using EQueue.Configurations;
using EQueue.NameServer;

namespace ENode.PublishEventPerfTests
{
    public static class ENodeExtensions
    {
        private static NameServerController _nameServerController;
        private static BrokerController _broker;
        private static DomainEventPublisher _eventPublisher;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            var brokerStorePath = @"c:\equeue-store";

            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            configuration.RegisterEQueueComponents();
            _nameServerController = new NameServerController();
            _broker = BrokerController.Create();
            _eventPublisher = new DomainEventPublisher();
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_eventPublisher);
            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _nameServerController.Start();
            _broker.Start();
            _eventPublisher.Start();
            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _eventPublisher.Shutdown();
            _broker.Shutdown();
            _nameServerController.Shutdown();
            return enodeConfiguration;
        }
    }
}
