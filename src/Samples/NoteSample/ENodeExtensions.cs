using System.Linq;
using System.Threading;
using ECommon.IoC;
using ECommon.Scheduling;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Eventing;
using EQueue.Broker;
using EQueue.Clients.Consumers;
using EQueue.Configurations;

namespace NoteSample.EQueueIntegrations
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;
        private static EventPublisher _eventPublisher;
        private static EventConsumer _eventConsumer;
        private static FailedCommandMessageSender _failedCommandMessageSender;
        private static DomainEventHandledMessageSender _domainEventHandledMessageSender;
        private static CommandResultProcessor _commandResultProcessor;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();
            configuration.SetDefault<ICommandTopicProvider, CommandTopicManager>();
            configuration.SetDefault<IEventTopicProvider, EventTopicManager>();
            configuration.SetDefault<ICommandTypeCodeProvider, CommandTypeCodeManager>();
            configuration.SetDefault<IEventTypeCodeProvider, EventTypeCodeManager>();

            var consumerSetting = new ConsumerSetting
            {
                HeartbeatBrokerInterval = 1000,
                UpdateTopicQueueCountInterval = 1000,
                RebalanceInterval = 1000
            };
            var eventConsumerSetting = new ConsumerSetting
            {
                HeartbeatBrokerInterval = 1000,
                UpdateTopicQueueCountInterval = 1000,
                RebalanceInterval = 1000,
                MessageHandleMode = MessageHandleMode.Sequential
            };

            _broker = new BrokerController().Initialize();

            var failedCommandMessageConsumer = new Consumer(consumerSetting, "FailedCommandMessageConsumer", "FailedCommandMessageConsumerGroup_" + ObjectId.GenerateNewId().ToString());
            var domainEventHandledMessageConsumer = new Consumer(consumerSetting, "DomainEventHandledMessageConsumer", "DomainEventHandledMessageConsumerGroup_" + ObjectId.GenerateNewId().ToString());
            _commandResultProcessor = new CommandResultProcessor(failedCommandMessageConsumer, domainEventHandledMessageConsumer);

            _commandService = new CommandService(_commandResultProcessor);
            _failedCommandMessageSender = new FailedCommandMessageSender();
            _domainEventHandledMessageSender = new DomainEventHandledMessageSender();
            _eventPublisher = new EventPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IEventPublisher, EventPublisher>(_eventPublisher);

            _commandConsumer = new CommandConsumer(consumerSetting, _failedCommandMessageSender);
            _eventConsumer = new EventConsumer(eventConsumerSetting, _domainEventHandledMessageSender);

            _commandConsumer.Subscribe("NoteCommandTopic");
            _eventConsumer.Subscribe("NoteEventTopic");
            _commandResultProcessor.SetFailedCommandMessageTopic("FailedCommandMessageTopic");
            _commandResultProcessor.SetDomainEventHandledMessageTopic("DomainEventHandledMessageTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _eventConsumer.Start();
            _commandConsumer.Start();
            _eventPublisher.Start();
            _commandService.Start();
            _failedCommandMessageSender.Start();
            _domainEventHandledMessageSender.Start();
            _commandResultProcessor.Start();

            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }

        private static void WaitAllConsumerLoadBalanceComplete()
        {
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);
            var taskId = scheduleService.ScheduleTask(() =>
            {
                var eventConsumerAllocatedQueues = _eventConsumer.Consumer.GetCurrentQueues();
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                var failedCommandMessageConsumerAllocatedQueues = _commandResultProcessor.FailedCommandMessageConsumer.GetCurrentQueues();
                var domainEventHandledMessageConsumerAllocatedQueues = _commandResultProcessor.DomainEventHandledMessageConsumer.GetCurrentQueues();
                if (eventConsumerAllocatedQueues.Count() == 4
                    && commandConsumerAllocatedQueues.Count() == 4
                    && failedCommandMessageConsumerAllocatedQueues.Count() == 4
                    && domainEventHandledMessageConsumerAllocatedQueues.Count() == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.ShutdownTask(taskId);
        }
    }
}
