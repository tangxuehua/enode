using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.Eventing;

namespace NoteSample.EQueueIntegrations
{
    public static class EQueueConfigurations
    {
        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            var commandService = new CommandService();
            var commandConsumer = new CommandConsumer();
            var eventPublisher = new EventPublisher();
            var eventConsumer = new EventConsumer();

            configuration.SetDefault<ICommandService, CommandService>(commandService);
            configuration.SetDefault<IEventPublisher, EventPublisher>(eventPublisher);

            commandService.Start();
            commandConsumer.Start();
            eventPublisher.Start();
            eventConsumer.Start();

            return enodeConfiguration;
        }
    }
}
