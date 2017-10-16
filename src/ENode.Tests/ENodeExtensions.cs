using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ECommon.Socketing;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Broker;
using EQueue.Clients.Consumers;
using EQueue.Configurations;
using EQueue.NameServer;
using EQueue.Protocols;

namespace ENode.Tests
{
    public static class ENodeExtensions
    {
        private static NameServerController _nameServerController;
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;
        private static DomainEventPublisher _eventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static ApplicationMessagePublisher _applicationMessagePublisher;
        private static ApplicationMessageConsumer _applicationMessageConsumer;
        private static PublishableExceptionPublisher _publishableExceptionPublisher;
        private static PublishableExceptionConsumer _publishableExceptionConsumer;

        public static ENodeConfiguration BuildContainer(this ENodeConfiguration enodeConfiguration)
        {
            enodeConfiguration.GetCommonConfiguration().BuildContainer();
            return enodeConfiguration;
        }
        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration,
            bool useMockDomainEventPublisher = false,
            bool useMockApplicationMessagePublisher = false,
            bool useMockPublishableExceptionPublisher = false)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();

            _commandService = new CommandService();
            _eventPublisher = new DomainEventPublisher();
            _applicationMessagePublisher = new ApplicationMessagePublisher();
            _publishableExceptionPublisher = new PublishableExceptionPublisher();

            if (useMockDomainEventPublisher)
            {
                configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, MockDomainEventPublisher>();
            }
            else
            {
                configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_eventPublisher);
            }

            if (useMockApplicationMessagePublisher)
            {
                configuration.SetDefault<IMessagePublisher<IApplicationMessage>, MockApplicationMessagePublisher>();
            }
            else
            {
                configuration.SetDefault<IMessagePublisher<IApplicationMessage>, ApplicationMessagePublisher>(_applicationMessagePublisher);
            }

            if (useMockPublishableExceptionPublisher)
            {
                configuration.SetDefault<IMessagePublisher<IPublishableException>, MockPublishableExceptionPublisher>();
            }
            else
            {
                configuration.SetDefault<IMessagePublisher<IPublishableException>, PublishableExceptionPublisher>(_publishableExceptionPublisher);
            }

            configuration.SetDefault<ICommandService, CommandService>(_commandService);

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var brokerStorePath = @"c:\equeue-store-ut";
            var brokerSetting = new BrokerSetting(chunkFileStoreRootPath: brokerStorePath);

            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            _nameServerController = new NameServerController();
            _broker = BrokerController.Create(brokerSetting);

            _commandService.Initialize(new CommandResultProcessor().Initialize(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9001)));
            _eventPublisher.Initialize();
            _applicationMessagePublisher.Initialize();
            _publishableExceptionPublisher.Initialize();

            _commandConsumer = new CommandConsumer().Initialize(setting: new ConsumerSetting { ConsumeFromWhere = ConsumeFromWhere.FirstOffset }).Subscribe("CommandTopic");
            _eventConsumer = new DomainEventConsumer().Initialize(setting: new ConsumerSetting { ConsumeFromWhere = ConsumeFromWhere.FirstOffset }).Subscribe("EventTopic");
            _applicationMessageConsumer = new ApplicationMessageConsumer().Initialize(setting: new ConsumerSetting { ConsumeFromWhere = ConsumeFromWhere.FirstOffset }).Subscribe("ApplicationMessageTopic");
            _publishableExceptionConsumer = new PublishableExceptionConsumer().Initialize(setting: new ConsumerSetting { ConsumeFromWhere = ConsumeFromWhere.FirstOffset }).Subscribe("PublishableExceptionTopic");

            _nameServerController.Start();
            _broker.Start();
            _eventConsumer.Start();
            _commandConsumer.Start();
            _applicationMessageConsumer.Start();
            _publishableExceptionConsumer.Start();
            _applicationMessagePublisher.Start();
            _publishableExceptionPublisher.Start();
            _eventPublisher.Start();
            _commandService.Start();
            WaitAllConsumerLoadBalanceComplete();
            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _eventPublisher.Shutdown();
            _applicationMessagePublisher.Shutdown();
            _publishableExceptionPublisher.Shutdown();
            _commandConsumer.Shutdown();
            _eventConsumer.Shutdown();
            _applicationMessageConsumer.Shutdown();
            _publishableExceptionConsumer.Shutdown();
            _broker.Shutdown();
            _nameServerController.Shutdown();
            return enodeConfiguration;
        }
        public static ENodeConfiguration UseEventStore(this ENodeConfiguration enodeConfiguration, bool useMockEventStore = false)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            if (useMockEventStore)
            {
                configuration.SetDefault<IEventStore, MockEventStore>();
            }
            else
            {
                enodeConfiguration.UseSqlServerEventStore();
            }
            return enodeConfiguration;
        }
        public static ENodeConfiguration UsePublishedVersionStore(this ENodeConfiguration enodeConfiguration, bool useMockPublishedVersionStore = false)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            if (useMockPublishedVersionStore)
            {
                configuration.SetDefault<IPublishedVersionStore, MockPublishedVersionStore>();
            }
            else
            {
                enodeConfiguration.UseSqlServerPublishedVersionStore();
            }
            return enodeConfiguration;
        }

        private static void WaitAllConsumerLoadBalanceComplete()
        {
            var logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(ENodeExtensions).Name);
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);
            logger.Info("Waiting for all consumer load balance complete, please wait for a moment...");
            scheduleService.StartTask("WaitAllConsumerLoadBalanceComplete", () =>
            {
                if (_eventConsumer.Consumer.GetCurrentQueues().Count() == 4
                 && _commandConsumer.Consumer.GetCurrentQueues().Count() == 4
                 && _applicationMessageConsumer.Consumer.GetCurrentQueues().Count() == 4
                 && _publishableExceptionConsumer.Consumer.GetCurrentQueues().Count() == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.StopTask("WaitAllConsumerLoadBalanceComplete");
            logger.Info("All consumer load balance completed.");
        }
    }
}
