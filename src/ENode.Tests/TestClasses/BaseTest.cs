using System.Configuration;
using System.Reflection;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace ENode.Tests
{
    public abstract class BaseTest
    {
        private static ENodeConfiguration _enodeConfiguration;
        protected static ILogger _logger;
        protected static ICommandService _commandService;
        protected static IMemoryCache _memoryCache;
        protected static ICommandStore _commandStore;
        protected static IEventStore _eventStore;
        protected static IPublishedVersionStore _publishedVersionStore;
        protected static IMessagePublisher<DomainEventStreamMessage> _domainEventPublisher;
        protected static IMessagePublisher<IApplicationMessage> _applicationMessagePublisher;
        protected static IMessagePublisher<IPublishableException> _publishableExceptionPublisher;

        protected static void Initialize(TestContext context,
            bool useMockCommandStore = false,
            bool useMockEventStore = false,
            bool useMockPublishedVersionStore = false,
            bool useMockDomainEventPublisher = false,
            bool useMockApplicationMessagePublisher = false,
            bool useMockPublishableExceptionPublisher = false)
        {
            if (_enodeConfiguration != null)
            {
                CleanupEnode();
            }
            InitializeENode(useMockCommandStore,
                useMockEventStore,
                useMockPublishedVersionStore,
                useMockDomainEventPublisher,
                useMockApplicationMessagePublisher,
                useMockPublishableExceptionPublisher);
        }

        private static void InitializeENode(
            bool useMockCommandStore = false,
            bool useMockEventStore = false,
            bool useMockPublishedVersionStore = false,
            bool useMockDomainEventPublisher = false,
            bool useMockApplicationMessagePublisher = false,
            bool useMockPublishableExceptionPublisher = false)
        {
            var setting = new ConfigurationSetting(ConfigurationManager.AppSettings["connectionString"]);
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly()
            };

            _enodeConfiguration = ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode(setting)
                .RegisterENodeComponents()
                .UseCommandStore(useMockCommandStore)
                .UseEventStore(useMockEventStore)
                .UsePublishedVersionStore(useMockPublishedVersionStore)
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies)
                .UseEQueue(useMockDomainEventPublisher, useMockApplicationMessagePublisher, useMockPublishableExceptionPublisher)
                .StartEQueue()
                .Start();

            _commandService = ObjectContainer.Resolve<ICommandService>();
            _memoryCache = ObjectContainer.Resolve<IMemoryCache>();
            _commandStore = ObjectContainer.Resolve<ICommandStore>();
            _eventStore = ObjectContainer.Resolve<IEventStore>();
            _publishedVersionStore = ObjectContainer.Resolve<IPublishedVersionStore>();
            _domainEventPublisher = ObjectContainer.Resolve<IMessagePublisher<DomainEventStreamMessage>>();
            _applicationMessagePublisher = ObjectContainer.Resolve<IMessagePublisher<IApplicationMessage>>();
            _publishableExceptionPublisher = ObjectContainer.Resolve<IMessagePublisher<IPublishableException>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(BaseTest));
            _logger.Info("ENode initialized.");
        }
        private static void CleanupEnode()
        {
            _enodeConfiguration.ShutdownEQueue();
            _enodeConfiguration.Stop();
            _logger.Info("ENode shutdown.");
        }
    }
}
