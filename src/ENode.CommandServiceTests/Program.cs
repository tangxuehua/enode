using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.IO;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using NoteSample.Commands;
using NoteSample.Domain;
using NUnit.Framework;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace ENode.CommandServiceTests
{
    class Program
    {
        static ENodeConfiguration _configuration;
        static ICommandService _commandService;
        static IMemoryCache _memoryCache;

        static void Main(string[] args)
        {
            InitializeENodeFramework();
            Console.WriteLine("ENode started....");

            create_and_update_aggregate_test();
            create_and_concurrent_update_aggregate_test();
            duplicate_create_aggregate_command_test();
            duplicate_update_aggregate_command_test();

            Console.WriteLine("");
            Console.WriteLine("All cases run success, press enter to exit.");
            Console.ReadLine();
        }
        static void InitializeENodeFramework()
        {
            var setting = new ConfigurationSetting
            {
                SqlServerDefaultConnectionString = ConfigurationManager.AppSettings["connectionString"],
                EnableGroupCommitEvent = false
            };
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.Load("NoteSample.CommandHandlers"),
                Assembly.GetExecutingAssembly()
            };
            _configuration = ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode(setting)
                .RegisterENodeComponents()
                .RegisterAllTypeCodes()
                .UseSqlServerEventStore()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies)
                .UseEQueue()
                .StartEQueue();

            _commandService = ObjectContainer.Resolve<ICommandService>();
            _memoryCache = ObjectContainer.Resolve<IMemoryCache>();
        }

        static void create_and_update_aggregate_test()
        {
            Console.WriteLine("");
            Console.WriteLine("----create_and_update_aggregate_test start.");
            var noteId = ObjectId.GenerateNewStringId();
            var command = new CreateNoteCommand
            {
                AggregateRootId = noteId,
                Title = "Sample Note"
            };

            //执行创建聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<Note>(noteId);
            Assert.NotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //执行修改聚合根的命令
            var command2 = new ChangeNoteTitleCommand
            {
                AggregateRootId = noteId,
                Title = "Changed Note"
            };
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            note = _memoryCache.Get<Note>(noteId);
            Assert.NotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
            Console.WriteLine("----create_and_update_aggregate_test end.");
        }
        static void duplicate_create_aggregate_command_test()
        {
            Console.WriteLine("");
            Console.WriteLine("----duplicate_create_aggregate_command_test start.");
            var noteId = ObjectId.GenerateNewStringId();
            var command = new CreateNoteCommand
            {
                AggregateRootId = noteId,
                Title = "Sample Note"
            };

            //执行创建聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<Note>(noteId);
            Assert.NotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //再次执行创建聚合根的命令
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);
            Console.WriteLine("----duplicate_create_aggregate_command_test end.");
        }
        static void duplicate_update_aggregate_command_test()
        {
            Console.WriteLine("");
            Console.WriteLine("----duplicate_update_aggregate_command_test start.");
            var noteId = ObjectId.GenerateNewStringId();
            var command1 = new CreateNoteCommand
            {
                AggregateRootId = noteId,
                Title = "Sample Note"
            };

            //先创建一个聚合根
            var status = _commandService.ExecuteAsync(command1).Result.Data.Status;
            Assert.AreEqual(CommandStatus.Success, status);

            var command2 = new ChangeNoteTitleCommand
            {
                AggregateRootId = noteId,
                Title = "Changed Note"
            };

            //执行修改聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<Note>(noteId);
            Assert.NotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);

            //在用重复执行该命令
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            note = _memoryCache.Get<Note>(noteId);
            Assert.NotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
            Console.WriteLine("----duplicate_update_aggregate_command_test end.");
        }
        static void create_and_concurrent_update_aggregate_test()
        {
            Console.WriteLine("");
            Console.WriteLine("----create_and_concurrent_update_aggregate_test start.");
            var noteId = ObjectId.GenerateNewStringId();
            var command = new CreateNoteCommand
            {
                AggregateRootId = noteId,
                Title = "Sample Note"
            };

            //执行创建聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<Note>(noteId);
            Assert.NotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //并发执行修改聚合根的命令
            var totalCount = 100;
            var finishedCount = 0;
            var waitHandle = new ManualResetEvent(false);
            for (var i = 0; i < totalCount; i++)
            {
                _commandService.ExecuteAsync(new ChangeNoteTitleCommand
                {
                    AggregateRootId = noteId,
                    Title = "Changed Note"
                }).ContinueWith(t =>
                {
                    var result = t.Result;
                    Assert.NotNull(result);
                    Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
                    Assert.NotNull(result.Data);
                    Assert.AreEqual(CommandStatus.Success, result.Data.Status);

                    var current = Interlocked.Increment(ref finishedCount);
                    if (current == totalCount)
                    {
                        note = _memoryCache.Get<Note>(noteId);
                        Assert.NotNull(note);
                        Assert.AreEqual("Changed Note", note.Title);
                        Assert.AreEqual(totalCount + 1, ((IAggregateRoot)note).Version);
                        waitHandle.Set();
                    }
                });
            }
            waitHandle.WaitOne();
            Console.WriteLine("----create_and_concurrent_update_aggregate_test end.");
        }
    }
}
