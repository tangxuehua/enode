using System;
using System.Reflection;
using ENode;
using ENode.Autofac;
using ENode.Commanding;
using ENode.Infrastructure;
using ENode.JsonNet;
using ENode.Log4Net;
using NoteSample.Commands;

namespace NoteSample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            var noteId = Guid.NewGuid();

            var command1 = new CreateNote(noteId, "Sample Note");
            var command2 = new ChangeNoteTitle(noteId, "Modified Note");

            var task = commandService.Send(command1);
            task.Wait();

            task = commandService.Send(command2);
            task.Wait();

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
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
