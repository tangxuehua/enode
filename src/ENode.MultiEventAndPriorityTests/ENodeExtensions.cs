using System.Linq;
using System.Net;
using System.Threading;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;
using EQueue.Broker;
using EQueue.Configurations;
using NoteSample.Commands;
using NoteSample.Domain;
using ENode.MultiEventAndPriorityTests.EventHandlers;

namespace ENode.MultiEventAndPriorityTests
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;
        private static DomainEventPublisher _eventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static CommandResultProcessor _commandResultProcessor;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();

            _broker = BrokerController.Create();

            _commandResultProcessor = new CommandResultProcessor(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9000));
            _commandService = new CommandService(_commandResultProcessor);
            _eventPublisher = new DomainEventPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_eventPublisher);

            //注意，这里实例化之前，需要确保各种MessagePublisher要先注入到IOC，因为CommandConsumer, DomainEventConsumer都依赖于IMessagePublisher<T>
            _commandConsumer = new CommandConsumer();
            _eventConsumer = new DomainEventConsumer();

            _commandConsumer.Subscribe("NoteCommandTopic");
            _eventConsumer.Subscribe("NoteEventTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _eventConsumer.Start();
            _commandConsumer.Start();
            _eventPublisher.Start();
            _commandService.Start();

            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _eventPublisher.Shutdown();
            _commandConsumer.Shutdown();
            _eventConsumer.Shutdown();
            _broker.Shutdown();
            return enodeConfiguration;
        }

        public static ENodeConfiguration RegisterAllTypeCodes(this ENodeConfiguration enodeConfiguration)
        {
            var provider = ObjectContainer.Resolve<ITypeCodeProvider>() as DefaultTypeCodeProvider;

            //aggregates
            provider.RegisterType<Note>(1000);

            //commands
            provider.RegisterType<CreateNoteCommand>(2000);
            provider.RegisterType<TestEventsCommand>(2001);

            //events
            provider.RegisterType<NoteCreated>(3001);
            provider.RegisterType<Event1>(3002);
            provider.RegisterType<Event2>(3003);
            provider.RegisterType<Event3>(3004);

            //event handlers
            provider.RegisterType<Handler1>(4001);
            provider.RegisterType<Handler2>(4002);
            provider.RegisterType<Handler3>(4003);

            provider.RegisterType<Handler121>(4004);
            provider.RegisterType<Handler122>(4005);
            provider.RegisterType<Handler123>(4006);

            provider.RegisterType<Handler1231>(4007);
            provider.RegisterType<Handler1232>(4008);
            provider.RegisterType<Handler1233>(4009);

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
                if (eventConsumerAllocatedQueues.Count() == 4 && commandConsumerAllocatedQueues.Count() == 4)
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
