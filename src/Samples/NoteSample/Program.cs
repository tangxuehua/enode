using System;
using System.Reflection;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.IoC;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ENode.Commanding;
using ENode.Configurations;
using NoteSample.Commands;
using NoteSample.EQueueIntegrations;

namespace NoteSample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            for (var index = 1; index <= 5; index++)
            {
                commandService.Send(new CreateNoteCommand(Guid.NewGuid(), "Sample Note" + index));
            }

            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofac()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .UseEQueue()
                .InitializeENode(assemblies)
                .StartEnode()
                .StartEQueue();
        }
    }
}
