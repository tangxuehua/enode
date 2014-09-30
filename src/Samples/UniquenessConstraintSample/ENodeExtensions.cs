using System.Linq;
using System.Threading;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Broker;
using EQueue.Configurations;
using UniquenessConstraintSample.Providers;

namespace UniquenessConstraintSample
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;
        private static EventPublisher _eventPublisher;
        private static CommandResultProcessor _commandResultProcessor;

        public static ENodeConfiguration UseSqlServerSectionIndexStore(this ENodeConfiguration enodeConfiguration, string connectionString, string sectionIndexTable)
        {
            enodeConfiguration.GetCommonConfiguration().SetDefault<ISectionIndexStore, SqlServerSectionIndexStore>(new SqlServerSectionIndexStore(connectionString, sectionIndexTable));
            return enodeConfiguration;
        }
        public static ENodeConfiguration SetProviders(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.SetDefault<ITopicProvider<ICommand>, CommandTopicProvider>();
            configuration.SetDefault<ITopicProvider<IEvent>, EventTopicProvider>();
            configuration.SetDefault<ITypeCodeProvider<ICommand>, CommandTypeCodeProvider>();
            configuration.SetDefault<ITypeCodeProvider<IAggregateRoot>, AggregateRootTypeCodeProvider>();
            configuration.SetDefault<ITypeCodeProvider<IEvent>, EventTypeCodeProvider>();
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
            configuration.SetDefault<IMessagePublisher<DomainEventStream>, EventPublisher>(_eventPublisher);

            _commandConsumer = new CommandConsumer();

            _commandConsumer.Subscribe("SectionCommandTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _commandConsumer.Start();
            _eventPublisher.Start();
            _commandService.Start();

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
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                var executedCommandMessageConsumerAllocatedQueues = _commandResultProcessor.CommandExecutedMessageConsumer.GetCurrentQueues();
                var domainEventHandledMessageConsumerAllocatedQueues = _commandResultProcessor.DomainEventHandledMessageConsumer.GetCurrentQueues();
                if (commandConsumerAllocatedQueues.Count() == 4
                    && executedCommandMessageConsumerAllocatedQueues.Count() == 4
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
