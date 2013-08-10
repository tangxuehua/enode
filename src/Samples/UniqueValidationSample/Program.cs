using System;
using System.Reflection;
using System.Threading;
using ENode;
using ENode.Autofac;
using ENode.Commanding;
using ENode.Infrastructure;
using ENode.JsonNet;
using ENode.Log4Net;
using ENode.Mongo;
using UniqueValidationSample.Commands;

namespace UniqueValidationSample {
    class Program {
        static void Main(string[] args) {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var suffix = Guid.NewGuid().ToString();

            var command1 = new RegisterUser { UserName = "netfocus_" + suffix };
            commandService.Execute(command1);

            var command2 = new RegisterUser { UserName = "netfocus_" + suffix };
            commandService.Send(command2, (result) => {
                if (result.HasError) {
                    Console.WriteLine("异常，用户名重复！");
                }
            });

            Thread.Sleep(1000);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        static void InitializeENodeFramework() {
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };
            var connectionString = "mongodb://localhost/UniqueValidationSampleDB";

            Configuration
                .Create()
                .UseAutofac()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .UseMongo(connectionString)
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
