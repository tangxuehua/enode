using System;
using System.Reflection;
using System.Threading;
using ENode;
using ENode.Commanding;
using ENode.Infrastructure;
using UniqueValidationSample.Commands;

namespace UniqueValidationSample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var suffix = Guid.NewGuid().ToString();

            var command1 = new RegisterUser { UserName = "netfocus_" + suffix };
            commandService.Execute(command1);

            var command2 = new RegisterUser { UserName = "netfocus_" + suffix };
            commandService.Send(command2, (result) =>
            {
                if (result.HasError)
                {
                    Console.WriteLine("异常，用户名重复！");
                }
            });

            Thread.Sleep(1000);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var connectionString = "mongodb://localhost/UniqueValidationSampleDB";
            var eventCollection = "Event";
            var eventPublishInfoCollection = "EventPublishInfo";
            var eventHandleInfoCollection = "EventHandleInfo";

            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofacContainer()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .SetDefault<ILoggerFactory, Log4NetLoggerFactory>(new Log4NetLoggerFactory("log4net.config"))
                .UseMongoAsStorage(connectionString, eventCollection, null, eventPublishInfoCollection, eventHandleInfoCollection)
                .CreateAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "EventQueue" })
                .Initialize(assemblies)
                .Start();
        }
    }
}
