using System.Net;
using ECommon.Components;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;
using EQueue.Configurations;
using NoteSample.Commands;

namespace DistributeSample.CommandProducer.EQueueIntegrations
{
    public static class ENodeExtensions
    {
        private static CommandService _commandService;
        private static CommandResultProcessor _commandResultProcessor;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();

            _commandResultProcessor = new CommandResultProcessor(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9000));
            _commandService = new CommandService(_commandResultProcessor);

            configuration.SetDefault<ICommandService, CommandService>(_commandService);

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Start();
            return enodeConfiguration;
        }

        public static ENodeConfiguration RegisterAllTypeCodes(this ENodeConfiguration enodeConfiguration)
        {
            var provider = ObjectContainer.Resolve<ITypeCodeProvider>() as DefaultTypeCodeProvider;

            //commands
            provider.RegisterType<CreateNoteCommand>(2000);

            return enodeConfiguration;
        }
    }
}
