using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using ENode.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.Commands;
using NoteSample.DomainEvents;

namespace NoteSample
{
    class Program
    {
        public static ManualResetEvent Signal = new ManualResetEvent(false);

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

            commandService.Send(command1);
            Signal.WaitOne();

            Signal = new ManualResetEvent(false);
            commandService.Send(command2);
            Signal.WaitOne();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }

    [Component]
    public class SyncService : IEventHandler<NoteCreated>, IEventHandler<NoteTitleChanged>
    {
        public void Handle(NoteCreated evnt)
        {
            Program.Signal.Set();
        }
        public void Handle(NoteTitleChanged evnt)
        {
            Program.Signal.Set();
        }
    }
}
