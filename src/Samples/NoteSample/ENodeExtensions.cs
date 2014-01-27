using ECommon.JsonNet;
using ECommon.Log4Net;
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
        private static CompletedCommandProcessor _completedCommandProcessor;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();
            configuration.SetDefault<ICommandTopicProvider, CommandTopicManager>();
            configuration.SetDefault<IEventTopicProvider, EventTopicManager>();
            configuration.SetDefault<ICommandTypeCodeProvider, CommandTypeCodeManager>();
            configuration.SetDefault<IEventTypeCodeProvider, EventTypeCodeManager>();

            var consumerSetting = ConsumerSetting.Default;
            {
                consumerSetting.HeartbeatBrokerInterval = 1000;
                consumerSetting.UpdateTopicQueueCountInterval = 1000;
                consumerSetting.RebalanceInterval = 1000;
            };

            _broker = new BrokerController().Initialize();
            _completedCommandProcessor = new CompletedCommandProcessor(consumerSetting);

            configuration.SetDefault<CompletedCommandProcessor, CompletedCommandProcessor>(_completedCommandProcessor);

            _commandService = new CommandService();
            _eventPublisher = new EventPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IEventPublisher, EventPublisher>(_eventPublisher);

            _commandConsumer = new CommandConsumer(consumerSetting);
            _eventConsumer = new EventConsumer(consumerSetting);

            _commandConsumer.Subscribe("NoteCommandTopic");
            _eventConsumer.Subscribe("NoteTopic");
            _completedCommandProcessor.Subscribe("NoteTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _eventConsumer.Start();
            _commandConsumer.Start();
            _eventPublisher.Start();
            _commandService.Start();
            _completedCommandProcessor.Start();

            return enodeConfiguration;
        }
        public static ENodeConfiguration UseLog4Net(this ENodeConfiguration enodeConfiguration)
        {
            enodeConfiguration.GetCommonConfiguration().UseLog4Net();
            return enodeConfiguration;
        }
        public static ENodeConfiguration UseLog4Net(this ENodeConfiguration enodeConfiguration, string configFile)
        {
            enodeConfiguration.GetCommonConfiguration().UseLog4Net(configFile);
            return enodeConfiguration;
        }
        public static ENodeConfiguration UseJsonNet(this ENodeConfiguration enodeConfiguration)
        {
            enodeConfiguration.GetCommonConfiguration().UseJsonNet();
            return enodeConfiguration;
        }
    }
}
