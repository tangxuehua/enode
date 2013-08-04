using System;
using System.Threading;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using NoteSample.Commands;

namespace NoteSample
{
    class Program
    {
        static void Main(string[] args)
        {
            new ENodeFrameworkUnitTestInitializer().Initialize();

            //如果要使用Sql或MongoDB来持久化，请用下面相应的语句来初始化
            //new ENodeFrameworkSqlInitializer().Initialize();
            //new ENodeFrameworkMongoInitializer().Initialize();

            //如果要使用Redis来作为内存缓存，请用下面相应的语句来初始化
            //new ENodeFrameworkRedisInitializer().Initialize();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var memoryCache = ObjectContainer.Resolve<IMemoryCache>();

            var noteId = Guid.NewGuid();

            var command1 = new CreateNote { NoteId = noteId, Title = "Sample Note" };
            var command2 = new ChangeNoteTitle { NoteId = noteId, Title = "Modified Note" };

            commandService.Send(command1, (result) => commandService.Send(command2));

            Thread.Sleep(1000);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
