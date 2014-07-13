using System;
using System.Reflection;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Extensions;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;

namespace UniquenessConstraintSample
{
    class Program
    {
        static ILogger _logger;
        const string ConnectionString = "Data Source=(local);Integrated Security=true;Initial Catalog=SampleDB;Connect Timeout=30;Min Pool Size=10;Max Pool Size=100";
        const string SectionIndexTable = "SectionIndex";

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var lockService = ObjectContainer.Resolve<ILockService>();
            lockService.AddLockKey(typeof(Section).Name);

            var memoryCache = ObjectContainer.Resolve<IMemoryCache>();
            var commandService = ObjectContainer.Resolve<ICommandService>();
            var sectionName = ObjectId.GenerateNewStringId();
            var command1 = new CreateSectionCommand(sectionName);
            var result = commandService.Execute(command1, CommandReturnType.CommandExecuted).WaitResult<CommandResult>(10000);
            var sectionId = result.AggregateRootId;
            _logger.Info("Section Name:" + memoryCache.Get<Section>(sectionId).Name);

            var command2 = new ChangeSectionNameCommand(sectionId, sectionName + "_2");
            commandService.Execute(command2, CommandReturnType.CommandExecuted).Wait();
            _logger.Info("Section Name:" + memoryCache.Get<Section>(sectionId).Name);

            Console.WriteLine(string.Empty);

            _logger.Info("Press Enter to exit...");

            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .CreateENode()
                .RegisterENodeComponents()
                .UseSqlServerLockService(ConnectionString)
                .UseSqlServerSectionIndexStore(ConnectionString, SectionIndexTable)
                .RegisterBusinessComponents(assemblies)
                .SetProviders()
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartENode()
                .StartEQueue();

            Console.WriteLine(string.Empty);

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _logger.Info("ENode started...");

            Console.WriteLine(string.Empty);
        }
    }
}
