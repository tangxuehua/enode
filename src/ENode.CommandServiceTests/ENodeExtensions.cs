using System.Net;
using ECommon.Components;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;
using EQueue.Broker;
using EQueue.Clients.Consumers;
using EQueue.Configurations;
using EQueue.Protocols;
using NoteSample.Commands;
using NoteSample.Domain;

namespace ENode.CommandServiceTests
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandConsumer _commandConsumer;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();
            configuration.RegisterEQueueComponents();
            _broker = BrokerController.Create();
            _commandService = new CommandService(new CommandResultProcessor(new IPEndPoint(SocketUtils.GetLocalIPV4(), 9001)));
            configuration.SetDefault<ICommandService, CommandService>(_commandService);

            _commandConsumer = new CommandConsumer(setting: new ConsumerSetting { ConsumeFromWhere = ConsumeFromWhere.FirstOffset }).Subscribe("NoteCommandTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _commandService.Start();
            _commandConsumer.Start();
            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandService.Shutdown();
            _commandConsumer.Shutdown();
            _broker.Shutdown();
            return enodeConfiguration;
        }
        public static ENodeConfiguration RegisterAllTypeCodes(this ENodeConfiguration enodeConfiguration)
        {
            var provider = ObjectContainer.Resolve<ITypeCodeProvider>() as DefaultTypeCodeProvider;

            //commands
            provider.RegisterType<CreateNoteCommand>(1000);
            provider.RegisterType<ChangeNoteTitleCommand>(1001);

            //aggregates
            provider.RegisterType<Note>(100);

            //events
            provider.RegisterType<NoteCreated>(2000);
            provider.RegisterType<NoteTitleChanged>(2001);

            return enodeConfiguration;
        }
    }
}
