using System.IO;
using System.Reflection;
using System.Threading;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.EQueue;
using ENode.Eventing;
using ENode.Messaging;
using EQueue.Broker;
using EQueue.Configurations;
using EQueue.NameServer;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.PublishEventPerfTests
{
    public static class ENodeExtensions
    {
        private static NameServerController _nameServerController;
        private static BrokerController _broker;
        private static DomainEventPublisher _eventPublisher;

        public static ENodeConfiguration BuildContainer(this ENodeConfiguration enodeConfiguration)
        {
            enodeConfiguration.GetCommonConfiguration().BuildContainer();
            return enodeConfiguration;
        }
        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            enodeConfiguration.RegisterTopicProviders(assemblies);

            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.RegisterEQueueComponents();

            _eventPublisher = new DomainEventPublisher();
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_eventPublisher);
            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var brokerStorePath = @"d:\publish-event-equeue-store";
            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            _eventPublisher.InitializeEQueue();

            _nameServerController = new NameServerController();
            _broker = BrokerController.Create(new BrokerSetting(chunkFileStoreRootPath: brokerStorePath));

            _nameServerController.Start();
            _broker.Start();
            _eventPublisher.Start();
            WaitAllProducerTopicQueuesAvailable();
            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _eventPublisher.Shutdown();
            _broker.Shutdown();
            _nameServerController.Shutdown();
            return enodeConfiguration;
        }

        private static void WaitAllProducerTopicQueuesAvailable()
        {
            var logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(ENodeExtensions).Name);
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);
            logger.Info("Waiting for all producer topic queues available, please wait for a moment...");
            scheduleService.StartTask("WaitAllProducerTopicQueuesAvailable", () =>
            {
                _eventPublisher.Producer.SendOneway(new EQueueMessage("NoteEventTopic", 100, new byte[1]), "1");
                var availableQueues = _eventPublisher.Producer.GetAvailableMessageQueues("NoteEventTopic");
                if (availableQueues != null && availableQueues.Count == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.StopTask("WaitAllProducerTopicQueuesAvailable");
            logger.Info("All producer topic queues are available.");
        }
    }
}
