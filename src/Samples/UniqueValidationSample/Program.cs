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

namespace UniqueValidationSample
{
    class Program
    {
        private const string ConnectionString = "mongodb://localhost/UniqueValidationSampleDB";

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var suffix = Guid.NewGuid().ToString();

            //var command1 = new RegisterUser { UserName = "netfocus_" + suffix };
            //commandService.Execute(command1);

            //var command2 = new RegisterUser { UserName = "netfocus_" + suffix };
            //commandService.Send(command2, (result) =>
            //{
            //    if (result.ErrorInfo != null)
            //    {
            //        Console.WriteLine("用户名重复！");
            //    }
            //});

            Thread.Sleep(1000);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofac()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .UseMongo(ConnectionString)
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
