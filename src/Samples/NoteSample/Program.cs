using System;
using System.Reflection;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using NoteSample.Commands;
using NoteSample.EQueueIntegrations;

namespace NoteSample
{
    class Program
    {
        static ILogger _logger;
        static ENodeConfiguration _configuration;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            for (var i = 0; i < 1000; i++)
            {
                commandService.SendAsync(new CreateNoteCommand
                {
                    AggregateRootId = ObjectId.GenerateNewStringId(),
                    Title = "Sample Title" + (i + 1).ToString()
                });
            }

            _logger.Info("Press Enter to exit...");

            Console.ReadLine();
            _configuration.ShutdownEQueue();
        }

        static void InitializeENodeFramework()
        {
            var connectionString = @"Server=(local);Initial Catalog=ENode;uid=sa;pwd=howareyou;Connect Timeout=30;Min Pool Size=10;Max Pool Size=100";
            var setting = new ConfigurationSetting
            {
                SqlServerDefaultConnectionString = connectionString,
                EnableGroupCommitEvent = true,
                CommandProcessorParallelThreadCount = 100
            };
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            _configuration = Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .CreateENode(setting)
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseSqlServerEventStore()
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartENode(NodeType.CommandProcessor | NodeType.EventProcessor)
                .StartEQueue();

            Console.WriteLine(string.Empty);

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _logger.Info("ENode started...");
        }
    }
}
