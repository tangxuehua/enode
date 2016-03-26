using System.Threading;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Domain;
using NoteSample.Commands;
using NoteSample.Domain;
using NUnit.Framework;

namespace ENode.Tests.TestCases
{
    public class CommandServiceTest
    {
        private readonly ICommandService _commandService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public CommandServiceTest()
        {
            _commandService = ObjectContainer.Resolve<ICommandService>();
            _memoryCache = ObjectContainer.Resolve<IMemoryCache>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(CommandServiceTest));
        }

        public void create_and_update_aggregate_test()
        {
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
        }
        public void duplicate_create_aggregate_command_test()
        {
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

            //用同一个命令再次执行创建聚合根的命令
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //用另一个命令再次执行创建相同聚合根的命令
            command = new CreateNoteCommand
            {
                AggregateRootId = noteId,
                Title = "Sample Note"
            };
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);
        }
        public void duplicate_update_aggregate_command_test()
        {
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
        }
        public void create_and_concurrent_update_aggregate_test()
        {
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
                var updateCommand = new ChangeNoteTitleCommand
                {
                    AggregateRootId = noteId,
                    Title = "Changed Note"
                };
                _commandService.ExecuteAsync(updateCommand).ContinueWith(t =>
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
        }
        public void change_nothing_test()
        {
            var command = new ChangeNothingCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.NothingChanged, commandResult.Status);
        }
        public void change_multiple_aggregates_test()
        {
            var command1 = new CreateNoteCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note1"
            };
            _commandService.ExecuteAsync(command1).Wait();

            var command2 = new CreateNoteCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note2"
            };
            _commandService.ExecuteAsync(command2).Wait();

            var command3 = new ChangeMultipleAggregatesCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                AggregateRootId1 = command1.AggregateRootId,
                AggregateRootId2 = command2.AggregateRootId
            };
            var asyncResult = _commandService.ExecuteAsync(command3).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        public void no_handler_command_test()
        {
            var command = new NoHandlerCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        public void two_handlers_command_test()
        {
            var command = new TwoHandlersCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        public void handler_throw_exception_command_test()
        {
            var command = new ThrowExceptionCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        public void async_command_handler_test()
        {
            var command1 = new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);

            var command2 = new AsyncHandlerCommand2()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
        }
        public void async_command_handler_throw_exception_test()
        {
            var command = new ThrowExceptionAsyncCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        public void two_async_handlers_command_test()
        {
            var command = new TwoAsyncHandlersCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.NotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.NotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
    }
}
