using System;
using System.Reflection;
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
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            var noteId = Guid.NewGuid();
            var command1 = new CreateNoteCommand(noteId, "Note Version1");
            var command2 = new ChangeNoteTitleCommand(noteId, "Note Version2");

            commandService.Send(command1).ContinueWith(task1 =>
            {
                Console.WriteLine("Sent command1.");
                if (task1.Result.Status == CommandResultStatus.Success)
                {
                    commandService.Send(command2).ContinueWith(task2 =>
                    {
                        if (task2.Result.Status == CommandResultStatus.Success)
                        {
                            Console.WriteLine("Sent command2.");
                        }
                    });
                }
            });

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
