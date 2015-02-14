using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using EQueue.Broker;
using EQueue.Configurations;

namespace ENode.SendCommandPerfTests
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.RegisterEQueueComponents();
            _broker = new BrokerController();
            _commandService = new CommandService();
            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _commandService.Start();
            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _broker.Shutdown();
            return enodeConfiguration;
        }
    }
}
