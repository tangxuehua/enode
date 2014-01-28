using System;
using System.Linq;
using System.Threading;
using ECommon.IoC;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using EQueue.Clients.Consumers;
using EQueue.Configurations;

namespace DistributeSample.CommandProducer.EQueueIntegrations
{
    public static class ENodeExtensions
    {
        private static CommandService _commandService;
        private static CompletedCommandProcessor _completedCommandProcessor;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();
            configuration.SetDefault<ICommandTopicProvider, CommandTopicManager>();
            configuration.SetDefault<ICommandTypeCodeProvider, CommandTypeCodeManager>();

            var consumerSetting = ConsumerSetting.Default;
            {
                consumerSetting.HeartbeatBrokerInterval = 1000;
                consumerSetting.UpdateTopicQueueCountInterval = 1000;
                consumerSetting.RebalanceInterval = 1000;
            };

            _completedCommandProcessor = new CompletedCommandProcessor(consumerSetting);

            configuration.SetDefault<CompletedCommandProcessor, CompletedCommandProcessor>(_completedCommandProcessor);

            _commandService = new CommandService();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);

            _completedCommandProcessor.Subscribe("NoteEventTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Start();
            _completedCommandProcessor.Start();

            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }

        private static void WaitAllConsumerLoadBalanceComplete()
        {
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);
            var taskId = scheduleService.ScheduleTask(() =>
            {
                var completedCommandProcessorAllocatedQueues = _completedCommandProcessor.Consumer.GetCurrentQueues();
                if (completedCommandProcessorAllocatedQueues.Count() == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.ShutdownTask(taskId);
        }
    }
}
