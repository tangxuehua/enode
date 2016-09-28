using System.IO;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using EQueue.Broker;
using EQueue.Configurations;
using EQueue.NameServer;

namespace ENode.SendCommandPerfTests
{
    public static class ENodeExtensions
    {
        private static NameServerController _nameServerController;
        private static BrokerController _broker;
        private static CommandService _commandService;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            var brokerStorePath = @"c:\equeue-store";

            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            configuration.RegisterEQueueComponents();
            _nameServerController = new NameServerController();
            _broker = BrokerController.Create();
            _commandService = new CommandService();
            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _nameServerController.Start();
            _broker.Start();
            _commandService.Start();
            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _broker.Shutdown();
            _nameServerController.Shutdown();
            return enodeConfiguration;
        }
    }
}
