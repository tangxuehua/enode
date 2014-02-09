using System.Linq;
using System.Threading;
using ECommon.IoC;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.EQueue;
using ENode.Eventing;
using EQueue.Clients.Consumers;
using EQueue.Configurations;

namespace DistributeSample.CommandProcessor.EQueueIntegrations
{
    public static class ENodeExtensions
    {
        private static CommandConsumer _commandConsumer;
        private static FailedCommandMessageSender _failedCommandMessageSender;
        private static EventPublisher _eventPublisher;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();
            configuration.SetDefault<IEventTopicProvider, EventTopicManager>();
            configuration.SetDefault<ICommandTypeCodeProvider, CommandTypeCodeManager>();
            configuration.SetDefault<IEventTypeCodeProvider, EventTypeCodeManager>();

            var consumerSetting = new ConsumerSetting
            {
                HeartbeatBrokerInterval = 1000,
                UpdateTopicQueueCountInterval = 1000,
                RebalanceInterval = 1000
            };

            _failedCommandMessageSender = new FailedCommandMessageSender();
            _eventPublisher = new EventPublisher();

            configuration.SetDefault<IEventPublisher, EventPublisher>(_eventPublisher);

            _commandConsumer = new CommandConsumer(consumerSetting, _failedCommandMessageSender);

            _commandConsumer.Subscribe("NoteCommandTopic1");
            _commandConsumer.Subscribe("NoteCommandTopic2");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandConsumer.Start();
            _failedCommandMessageSender.Start();
            _eventPublisher.Start();

            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }

        private static void WaitAllConsumerLoadBalanceComplete()
        {
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);
            var taskId = scheduleService.ScheduleTask(() =>
            {
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                if (commandConsumerAllocatedQueues.Count() == 8)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.ShutdownTask(taskId);
        }
    }
}
