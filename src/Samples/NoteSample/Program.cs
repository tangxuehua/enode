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

            var noteId = Guid.NewGuid();
            var command1 = new CreateNoteCommand(noteId, "Note Version1");
            var command2 = new ChangeNoteTitleCommand(noteId, "Note Version2");

            commandService.Send(command1).ContinueWith(task =>
            {
                if (task.Result.Status == CommandResultStatus.Success)
                {
                    commandService.Send(command2);
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
