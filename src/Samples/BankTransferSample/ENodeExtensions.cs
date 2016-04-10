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
using EQueue.Configurations;

namespace BankTransferSample
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandResultProcessor _commandResultProcessor;
        private static CommandConsumer _commandConsumer;
        private static ApplicationMessagePublisher _applicationMessagePublisher;
        private static ApplicationMessageConsumer _applicationMessageConsumer;
        private static DomainEventPublisher _domainEventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static PublishableExceptionPublisher _exceptionPublisher;
        private static PublishableExceptionConsumer _exceptionConsumer;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            var brokerStorePath = @"d:\equeue-store";

            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            configuration.RegisterEQueueComponents();

            _broker = BrokerController.Create();

            _commandResultProcessor = new CommandResultProcessor(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9000));
            _commandService = new CommandService(_commandResultProcessor);
            _applicationMessagePublisher = new ApplicationMessagePublisher();
            _domainEventPublisher = new DomainEventPublisher();
            _exceptionPublisher = new PublishableExceptionPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IMessagePublisher<IApplicationMessage>, ApplicationMessagePublisher>(_applicationMessagePublisher);
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_domainEventPublisher);
            configuration.SetDefault<IMessagePublisher<IPublishableException>, PublishableExceptionPublisher>(_exceptionPublisher);

            _commandConsumer = new CommandConsumer().Subscribe("BankTransferCommandTopic");
            _applicationMessageConsumer = new ApplicationMessageConsumer().Subscribe("BankTransferApplicationMessageTopic");
            _eventConsumer = new DomainEventConsumer().Subscribe("BankTransferEventTopic");
            _exceptionConsumer = new PublishableExceptionConsumer().Subscribe("BankTransferExceptionTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _exceptionConsumer.Start();
            _eventConsumer.Start();
            _applicationMessageConsumer.Start();
            _commandConsumer.Start();
            _applicationMessagePublisher.Start();
            _domainEventPublisher.Start();
            _exceptionPublisher.Start();
            _commandService.Start();

            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _applicationMessagePublisher.Shutdown();
            _domainEventPublisher.Shutdown();
            _exceptionPublisher.Shutdown();
            _commandConsumer.Shutdown();
            _applicationMessageConsumer.Shutdown();
            _eventConsumer.Shutdown();
            _exceptionConsumer.Shutdown();
            _broker.Shutdown();
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
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                var eventConsumerAllocatedQueues = _eventConsumer.Consumer.GetCurrentQueues();
                var exceptionConsumerAllocatedQueues = _exceptionConsumer.Consumer.GetCurrentQueues();
                if (commandConsumerAllocatedQueues.Count() == 4 && eventConsumerAllocatedQueues.Count() == 4 && exceptionConsumerAllocatedQueues.Count() == 4)
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
