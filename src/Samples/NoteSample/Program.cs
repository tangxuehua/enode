using System;
using System.Reflection;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.IoC;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Utilities;
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

            var noteId = ObjectId.GenerateNewStringId();
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
            var connectionString = "Data Source=(local);Initial Catalog=EventStore;Integrated Security=True;Connect Timeout=30;Min Pool Size=10;Max Pool Size=100";
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .CreateENode()
                .RegisterENodeComponents()
                .UseSqlServerEventStore(connectionString)
                .RegisterBusinessComponents(assemblies)
                .SetProviders()
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartRetryCommandService()
                .StartWaitingCommandService()
                .StartEQueue();

            Console.WriteLine(string.Empty);
            Console.WriteLine("Enode Framework started.");
        }
    }
}
