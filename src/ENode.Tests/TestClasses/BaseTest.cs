using System.Configuration;
using System.Reflection;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
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
        protected static IMessagePublisher<IApplicationMessage> _applicationMessagePublisher;

        protected static void Initialize(TestContext context, bool forceInitialize = false)
        {
            if (forceInitialize || _enodeConfiguration == null)
            {
                InitializeENode();
            }
        }
        protected static void Cleanup()
        {
            if (_enodeConfiguration != null)
            {
                CleanupEnode();
            }
        }

        private static void InitializeENode()
        {
            var setting = new ConfigurationSetting(ConfigurationManager.AppSettings["connectionString"]);
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.Load("NoteSample.CommandHandlers"),
                Assembly.Load("NoteSample.Commands"),
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
                .UseMockCommandStore()
                .UseMockApplicationMessagePublisher()
                .UseSqlServerEventStore()
                .UseSqlServerPublishedVersionStore()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies)
                .UseEQueue()
                .StartEQueue();

            _commandService = ObjectContainer.Resolve<ICommandService>();
            _memoryCache = ObjectContainer.Resolve<IMemoryCache>();
            _commandStore = ObjectContainer.Resolve<ICommandStore>();
            _applicationMessagePublisher = ObjectContainer.Resolve<IMessagePublisher<IApplicationMessage>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(BaseTest));
            _logger.Info("ENode initialized.");
        }
        private static void CleanupEnode()
        {
            _enodeConfiguration.ShutdownEQueue();
            _logger.Info("ENode shutdown.");
        }
    }
}
