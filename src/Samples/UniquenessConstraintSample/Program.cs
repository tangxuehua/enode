using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Infrastructure;

namespace UniquenessConstraintSample
{
    class Program
    {
        static ILogger _logger;
        const string ConnectionString = "Data Source=(local);Integrated Security=true;Initial Catalog=SampleDB;Connect Timeout=30;Min Pool Size=10;Max Pool Size=100";
        const string SectionIndexTable = "SectionIndex";

        /// <summary>运行本程序前，请先创建一个SampleDB数据库，然后执行一下SqlServerTableGenerateSQL.sql
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var lockService = ObjectContainer.Resolve<ILockService>();
            lockService.AddLockKey(typeof(Section).Name);

            ConcurrentTest();

            Console.WriteLine(string.Empty);

            _logger.Info("Press Enter to exit...");

            Console.ReadLine();
        }

        static void ConcurrentTest()
        {
            var commandService = ObjectContainer.Resolve<ICommandService>();
            var sectionName = ObjectId.GenerateNewStringId();
            var count = 100;
            var tasks = new List<Task<CommandResult>>();

            for (var index = 0; index < count; index++)
            {
                tasks.Add(commandService.Execute(new CreateSectionCommand(sectionName), CommandReturnType.CommandExecuted));
            }

            Task.WaitAll(tasks.ToArray());
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
