using System.Configuration;
using System.Reflection;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Eventing;
using ENode.Messaging;
using ENode.SqlServer;
using NUnit.Framework;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace ENode.Tests
{
    public abstract class BaseTest
    {
        private ENodeConfiguration _enodeConfiguration;
        protected ILogger _logger;
        protected ICommandService _commandService;
        protected IMemoryCache _memoryCache;
        protected IEventStore _eventStore;
        protected IPublishedVersionStore _publishedVersionStore;
        protected IMessagePublisher<DomainEventStreamMessage> _domainEventPublisher;
        protected IMessagePublisher<IApplicationMessage> _applicationMessagePublisher;
        protected IMessagePublisher<IDomainException> _domainExceptionPublisher;

        protected void Initialize(
            bool useMockEventStore = false,
            bool useMockPublishedVersionStore = false,
            bool useMockDomainEventPublisher = false,
            bool useMockApplicationMessagePublisher = false,
            bool useMockDomainExceptionPublisher = false)
        {
            InitializeENode(useMockEventStore,
                useMockPublishedVersionStore,
                useMockDomainEventPublisher,
                useMockApplicationMessagePublisher,
                useMockDomainExceptionPublisher);
        }

        [SetUp]
        public void TestInitialize()
        {
            _logger.InfoFormat("----Start to run test: {0}", TestContext.CurrentContext.Test.Name);
        }
        [TearDown]
        public void TestTearDown()
        {
            _logger.InfoFormat("----Finished test: {0}", TestContext.CurrentContext.Test.Name);
        }

        [OneTimeTearDown]
        protected void Cleanup()
        {
            if (_enodeConfiguration != null)
            {
                CleanupEnode();
            }
        }

        private void InitializeENode(
            bool useMockEventStore = false,
            bool useMockPublishedVersionStore = false,
            bool useMockDomainEventPublisher = false,
            bool useMockApplicationMessagePublisher = false,
            bool useMockDomainExceptionPublisher = false)
        {
            var connectionString = ConfigurationManager.AppSettings["connectionString"];
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
                .CreateENode()
                .RegisterENodeComponents()
                .UseEventStore(useMockEventStore)
                .UsePublishedVersionStore(useMockPublishedVersionStore)
                .RegisterBusinessComponents(assemblies)
                .InitializeEQueue()
                .UseEQueue(useMockDomainEventPublisher, useMockApplicationMessagePublisher, useMockDomainExceptionPublisher)
                .BuildContainer();

            if (!useMockEventStore)
            {
                _enodeConfiguration.InitializeSqlServerEventStore(connectionString);
            }
            if (!useMockPublishedVersionStore)
            {
                _enodeConfiguration.InitializeSqlServerPublishedVersionStore(connectionString);
            }

            _enodeConfiguration
                .InitializeBusinessAssemblies(assemblies)
                .StartEQueue()
                .Start();

            _commandService = ObjectContainer.Resolve<ICommandService>();
            _memoryCache = ObjectContainer.Resolve<IMemoryCache>();
            _eventStore = ObjectContainer.Resolve<IEventStore>();
            _publishedVersionStore = ObjectContainer.Resolve<IPublishedVersionStore>();
            _domainEventPublisher = ObjectContainer.Resolve<IMessagePublisher<DomainEventStreamMessage>>();
            _applicationMessagePublisher = ObjectContainer.Resolve<IMessagePublisher<IApplicationMessage>>();
            _domainExceptionPublisher = ObjectContainer.Resolve<IMessagePublisher<IDomainException>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(BaseTest));
            _logger.Info("----ENode initialized.");
        }
        private void CleanupEnode()
        {
            _enodeConfiguration.Stop();
            _logger.Info("----ENode shutdown.");
        }
    }
}
