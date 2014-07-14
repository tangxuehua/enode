using System.Linq;
using System.Threading;
using BankTransferSample.Providers;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Eventing;
using EQueue.Broker;
using EQueue.Configurations;

namespace BankTransferSample
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static IProcessCommandSender _processCommandSender;
        private static CommandResultProcessor _commandResultProcessor;
        private static CommandConsumer _commandConsumer;
        private static EventPublisher _eventPublisher;
        private static EventConsumer _eventConsumer;

        public static ENodeConfiguration SetProviders(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.SetDefault<ICommandTopicProvider, CommandTopicProvider>();
            configuration.SetDefault<IEventTopicProvider, EventTopicProvider>();
            configuration.SetDefault<ICommandTypeCodeProvider, CommandTypeCodeProvider>();
            configuration.SetDefault<IAggregateRootTypeCodeProvider, AggregateRootTypeCodeProvider>();
            configuration.SetDefault<IEventTypeCodeProvider, EventTypeCodeProvider>();
            configuration.SetDefault<IEventHandlerTypeCodeProvider, EventHandlerTypeCodeProvider>();
            return enodeConfiguration;
        }
        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();

            _broker = new BrokerController();

            _commandResultProcessor = new CommandResultProcessor();
            _commandService = new CommandService(_commandResultProcessor);
            _eventPublisher = new EventPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IProcessCommandSender, CommandService>(_commandService);
            configuration.SetDefault<IEventPublisher, EventPublisher>(_eventPublisher);

            _commandConsumer = new CommandConsumer();
            _eventConsumer = new EventConsumer();

            _commandConsumer.Subscribe("BankTransferCommandTopic");
            _eventConsumer.Subscribe("BankTransferEventTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _eventConsumer.Start();
            _commandConsumer.Start();
            _eventPublisher.Start();
            _commandService.Start();
            _commandResultProcessor.Start();

            WaitAllConsumerLoadBalanceComplete();

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
                var commandExecutedMessageConsumerAllocatedQueues = _commandResultProcessor.CommandExecutedMessageConsumer.GetCurrentQueues();
                var domainEventHandledMessageConsumerAllocatedQueues = _commandResultProcessor.DomainEventHandledMessageConsumer.GetCurrentQueues();
                if (eventConsumerAllocatedQueues.Count() == 4
                    && commandConsumerAllocatedQueues.Count() == 4
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
