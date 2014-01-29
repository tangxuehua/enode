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

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();
            configuration.SetDefault<IEventTypeCodeProvider, EventTypeCodeManager>();

            var eventConsumerSetting = ConsumerSetting.Default;
            {
                eventConsumerSetting.HeartbeatBrokerInterval = 1000;
                eventConsumerSetting.UpdateTopicQueueCountInterval = 1000;
                eventConsumerSetting.RebalanceInterval = 1000;
                eventConsumerSetting.MessageHandleMode = MessageHandleMode.Sequential;
            };

            _eventConsumer = new EventConsumer(eventConsumerSetting);

            _eventConsumer.Subscribe("NoteEventTopic1");
            _eventConsumer.Subscribe("NoteEventTopic2");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _eventConsumer.Start();

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
