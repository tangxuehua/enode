using System;
using System.Reflection;
using DistributeEventStoreSample.Client.Commands;
using DistributeEventStoreSample.Client.EQueueIntegrations;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.IoC;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ENode.Commanding;
using ENode.Configurations;

namespace DistributeEventStoreSample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            var noteId = Guid.NewGuid();
            var command1 = new CreateNoteCommand(noteId, "Sample Title1");
            var command2 = new ChangeNoteTitleCommand(noteId, "Sample Title2");

            Console.WriteLine(string.Empty);

            commandService.Execute(command1).Wait();
            commandService.Execute(command2).Wait();

            Console.WriteLine(string.Empty);

            Console.WriteLine("Press Enter to exit...");

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
                .UseRemotingEventStore()
                .RegisterBusinessComponents(assemblies)
                .SetEventTypeCodeProvider()
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartRetryCommandService()
                .StartWaitingCommandService()
                .StartEventStoreClient()
                .StartEQueue();

            Console.WriteLine(string.Empty);
            Console.WriteLine("Enode Framework started.");
        }
    }
}
