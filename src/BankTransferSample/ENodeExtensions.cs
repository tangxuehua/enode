using System.Linq;
using System.Threading;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Eventing;
using ENode.Exceptions;
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
        private static DomainEventPublisher _eventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static PublishableExceptionPublisher _exceptionPublisher;
        private static PublishableExceptionConsumer _exceptionConsumer;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();

            _broker = new BrokerController();

            _commandResultProcessor = new CommandResultProcessor();
            _commandService = new CommandService(_commandResultProcessor);
            _eventPublisher = new DomainEventPublisher();
            _exceptionPublisher = new PublishableExceptionPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IMessagePublisher<ApplicationCommandResult>, DomainEventPublisher>(_eventPublisher);
            configuration.SetDefault<IMessagePublisher<DomainEventStream>, DomainEventPublisher>(_eventPublisher);
            configuration.SetDefault<IMessagePublisher<IPublishableException>, PublishableExceptionPublisher>(_exceptionPublisher);

            _commandConsumer = new CommandConsumer();
            _eventConsumer = new DomainEventConsumer();
            _exceptionConsumer = new PublishableExceptionConsumer();

            _commandConsumer.Subscribe("BankTransferCommandTopic");
            _eventConsumer.Subscribe("BankTransferEventTopic");
            _exceptionConsumer.Subscribe("BankTransferExceptionTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _exceptionConsumer.Start();
            _eventConsumer.Start();
            _commandConsumer.Start();
            _exceptionPublisher.Start();
            _eventPublisher.Start();
            _commandService.Start();
            _commandResultProcessor.Start();

            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandResultProcessor.Shutdown();
            _commandService.Shutdown();
            _eventPublisher.Shutdown();
            _exceptionPublisher.Shutdown();
            _commandConsumer.Shutdown();
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
            var taskId = scheduleService.ScheduleTask("WaitAllConsumerLoadBalanceComplete", () =>
            {
                var eventConsumerAllocatedQueues = _eventConsumer.Consumer.GetCurrentQueues();
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                var exceptionConsumerAllocatedQueues = _exceptionConsumer.Consumer.GetCurrentQueues();
                var commandExecutedMessageConsumerAllocatedQueues = _commandResultProcessor.CommandExecutedMessageConsumer.GetCurrentQueues();
                var domainEventHandledMessageConsumerAllocatedQueues = _commandResultProcessor.DomainEventHandledMessageConsumer.GetCurrentQueues();
                if (eventConsumerAllocatedQueues.Count() == 4
                    && commandConsumerAllocatedQueues.Count() == 4
                    && exceptionConsumerAllocatedQueues.Count() == 4
                    && commandExecutedMessageConsumerAllocatedQueues.Count() == 4
                    && domainEventHandledMessageConsumerAllocatedQueues.Count() == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.ShutdownTask(taskId);
            logger.Info("All consumer load balance completed.");
        }
    }
}
