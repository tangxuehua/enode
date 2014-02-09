using System;
using System.Linq;
using System.Threading;
using ECommon.IoC;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.EQueue;
using EQueue.Clients.Consumers;
using EQueue.Configurations;

namespace DistributeSample.EventProcessor.EQueueIntegrations
{
    public static class ENodeExtensions
    {
        private static EventConsumer _eventConsumer;
        private static DomainEventHandledMessageSender _domainEventHandledMessageSender;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();
            configuration.SetDefault<IEventTypeCodeProvider, EventTypeCodeManager>();

            var eventConsumerSetting = new ConsumerSetting
            {
                HeartbeatBrokerInterval = 1000,
                UpdateTopicQueueCountInterval = 1000,
                RebalanceInterval = 1000,
                MessageHandleMode = MessageHandleMode.Sequential
            };

            _domainEventHandledMessageSender = new DomainEventHandledMessageSender();
            _eventConsumer = new EventConsumer(eventConsumerSetting, _domainEventHandledMessageSender);

            _eventConsumer.Subscribe("NoteEventTopic1");
            _eventConsumer.Subscribe("NoteEventTopic2");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _eventConsumer.Start();
            _domainEventHandledMessageSender.Start();
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
                if (eventConsumerAllocatedQueues.Count() == 8)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.ShutdownTask(taskId);
        }
    }
}
