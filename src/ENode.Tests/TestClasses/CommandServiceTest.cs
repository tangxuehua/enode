using System.Threading;
using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Domain;
using ENode.Tests.Commands;
using ENode.Tests.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class CommandServiceTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context);
        }

        #region Command Tests

        [TestMethod]
        public void create_and_update_aggregate_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };

            //执行创建聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //执行修改聚合根的命令
            var command2 = new ChangeTestAggregateTitleCommand
            {
                AggregateRootId = aggregateId,
                Title = "Changed Note"
            };
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
        }
        [TestMethod]
        public void command_sync_execute_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note",
                SleepMilliseconds = 3000
            };

            //执行创建聚合根的命令
            var commandResult = _commandService.Execute(command, 5000);
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);
        }
        [TestMethod]
        public void command_sync_execute_timeout_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note",
                SleepMilliseconds = 5000
            };

            //执行创建聚合根的命令
            var isTimeout = false;
            try
            {
                var commandResult = _commandService.Execute(command, 3000);
            }
            catch (CommandExecuteTimeoutException)
            {
                isTimeout = true;
            }
            Assert.IsTrue(isTimeout);
        }
        [TestMethod]
        public void duplicate_create_aggregate_command_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };

            //执行创建聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //用同一个命令再次执行创建聚合根的命令
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //用另一个命令再次执行创建相同聚合根的命令
            command = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);
        }
        [TestMethod]
        public void duplicate_update_aggregate_command_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command1 = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };

            //先创建一个聚合根
            var status = _commandService.ExecuteAsync(command1).Result.Data.Status;
            Assert.AreEqual(CommandStatus.Success, status);

            var command2 = new ChangeTestAggregateTitleCommand
            {
                AggregateRootId = aggregateId,
                Title = "Changed Note"
            };

            //执行修改聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command2, CommandReturnType.EventHandled).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);

            //在重复执行该命令
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
        }
        [TestMethod]
        public void create_and_concurrent_update_aggregate_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };

            //执行创建聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //并发执行修改聚合根的命令
            var totalCount = 100;
            var finishedCount = 0;
            var waitHandle = new ManualResetEvent(false);
            for (var i = 0; i < totalCount; i++)
            {
                var updateCommand = new ChangeTestAggregateTitleCommand
                {
                    AggregateRootId = aggregateId,
                    Title = "Changed Note"
                };
                _commandService.ExecuteAsync(updateCommand).ContinueWith(t =>
                {
                    var result = t.Result;
                    Assert.IsNotNull(result);
                    Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
                    Assert.IsNotNull(result.Data);
                    Assert.AreEqual(CommandStatus.Success, result.Data.Status);

                    var current = Interlocked.Increment(ref finishedCount);
                    if (current == totalCount)
                    {
                        note = _memoryCache.Get<TestAggregate>(aggregateId);
                        Assert.IsNotNull(note);
                        Assert.AreEqual("Changed Note", note.Title);
                        Assert.AreEqual(totalCount + 1, ((IAggregateRoot)note).Version);
                        waitHandle.Set();
                    }
                });
            }
            waitHandle.WaitOne();
        }
        [TestMethod]
        public void change_nothing_test()
        {
            var command = new ChangeNothingCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.NothingChanged, commandResult.Status);
        }
        [TestMethod]
        public void set_result_command_test()
        {
            var command = new SetResultCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Result = "CommandResult"
            };
            var asyncResult = _commandService.ExecuteAsync(command, CommandReturnType.EventHandled).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            Assert.AreEqual("CommandResult", commandResult.Result);
        }
        [TestMethod]
        public void change_multiple_aggregates_test()
        {
            var command1 = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note1"
            };
            _commandService.ExecuteAsync(command1).Wait();

            var command2 = new CreateTestAggregateCommand
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
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        [TestMethod]
        public void no_handler_command_test()
        {
            var command = new NoHandlerCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        [TestMethod]
        public void two_handlers_command_test()
        {
            var command = new TwoHandlersCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        [TestMethod]
        public void handler_throw_exception_command_test()
        {
            var command = new ThrowExceptionCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        [TestMethod]
        public void aggregate_throw_exception_command_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };

            _commandService.ExecuteAsync(command).Wait();

            var command1 = new AggregateThrowExceptionCommand
            {
                AggregateRootId = aggregateId,
                PublishableException = false
            };
            var asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);

            var command2 = new AggregateThrowExceptionCommand
            {
                AggregateRootId = aggregateId,
                PublishableException = true
            };
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        [TestMethod]
        public void command_inheritance_test()
        {
            var command = new BaseCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.NothingChanged, commandResult.Status);
            Assert.AreEqual("ResultFromBaseCommand", commandResult.Result);

            command = new ChildCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.NothingChanged, commandResult.Status);
            Assert.AreEqual("ResultFromChildCommand", commandResult.Result);
        }

        #endregion

        #region Async Command Tests

        [TestMethod]
        public void async_command_handler_test()
        {
            var command = new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
        }
        [TestMethod]
        public void async_command_handler_throw_exception_test()
        {
            var asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                ShouldThrowException = true
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);

            asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                ShouldThrowIOException = true
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
        }
        [TestMethod]
        public void async_command_two_handlers_test()
        {
            var command = new TwoAsyncHandlersCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        [TestMethod]
        public void duplicate_async_command_test()
        {
            var command = new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);

            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
        }
        [TestMethod]
        public void duplicate_async_command_with_application_message_test()
        {
            var command = new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                ShouldGenerateApplicationMessage = true
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);

            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
        }
        [TestMethod]
        public void duplicate_async_command_not_check_handler_exist_with_result_test()
        {
            var command = new NotCheckAsyncHandlerExistWithResultCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);

            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
        }
        [TestMethod]
        public void async_command_inheritance_test()
        {
            var command = new AsyncHandlerBaseCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);

            command = new AsyncHandlerChildCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            };
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
        }

        #endregion
    }
}
