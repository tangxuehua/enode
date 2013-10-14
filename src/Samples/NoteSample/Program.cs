using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using ENode.Commanding;
using ENode.Infrastructure;
using NoteSample.Commands;

namespace NoteSample
{
    class Program
    {
        static void Main(string[] args)
        {
            new ENodeFrameworkUnitTestInitializer().Initialize();
            //new ENodeFrameworkSqlInitializer().Initialize();
            //new ENodeFrameworkMongoInitializer().Initialize();
            //new ENodeFrameworkRedisInitializer().Initialize();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            var noteId = Guid.NewGuid();

            var command1 = new CreateNote { NoteId = noteId, Title = "Sample Note" };
            var command2 = new ChangeNoteTitle { NoteId = noteId, Title = "Modified Note" };

            commandService.Send(command1, result => commandService.Send(command2));

            Thread.Sleep(1000);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
