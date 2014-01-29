using System;
using System.Reflection;
using System.Threading;
using DistributeSample.CommandProducer.EQueueIntegrations;
using DistributeSample.Commands;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.IoC;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ENode.Commanding;
using ENode.Configurations;

namespace DistributeSample.CommandProducer
{
    class Program
    {
        static int _count;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            for (var index = 1; index <= 10; index++)
            {
                commandService.Send(new CreateNoteCommand(Guid.NewGuid(), "Sample Note" + index)).ContinueWith(task =>
                {
                    if (task.Result.Status == CommandResultStatus.Success)
                    {
                        Console.WriteLine("Sent command{0}.", Interlocked.Increment(ref _count));
                    }
                });
            }

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
                .RegisterBusinessComponents(assemblies)
                .UseEQueue()
                .InitializeENode(assemblies)
                .StartEQueue()
                .StartEnode();
        }
    }
}
