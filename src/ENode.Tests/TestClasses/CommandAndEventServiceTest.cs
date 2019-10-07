using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ECommon.Components;
using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Tests.Commands;
using ENode.Tests.Domain;
using NUnit.Framework;

namespace ENode.Tests
{
    [TestFixture]
    public class CommandAndEventServiceTest : BaseTest
    {
        public readonly static ConcurrentDictionary<int, IList<string>> HandlerTypes = new ConcurrentDictionary<int, IList<string>>();

        [OneTimeSetUp]
        public void ClassInitialize()
        {
            Initialize();
        }

        #region Command Tests

        [Test]
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
            NUnit.Framework.Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
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
            note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
        }
        [Test]
        public void create_and_update_inherit_aggregate_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var command = new CreateInheritTestAggregateCommand
            {
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };

            //执行创建聚合根的命令
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            NUnit.Framework.Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            var note = _memoryCache.GetAsync<InheritTestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //执行修改聚合根的命令
            var command2 = new ChangeInheritTestAggregateTitleCommand
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
            note = _memoryCache.GetAsync<InheritTestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
        }
        [Test]
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
            var commandResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Data.Status);
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //执行修改聚合根的命令
            var command2 = new ChangeTestAggregateTitleCommand
            {
                AggregateRootId = aggregateId,
                Title = "Changed Note"
            };
            commandResult = _commandService.ExecuteAsync(command2).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Data.Status);
            note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
        }
        [Test]
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
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
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
        [Test]
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
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
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
            note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual("Changed Note", note.Title);
            Assert.AreEqual(2, ((IAggregateRoot)note).Version);
        }
        [Test]
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
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
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
                        note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
                        Assert.IsNotNull(note);
                        Assert.AreEqual("Changed Note", note.Title);
                        Assert.AreEqual(totalCount + 1, ((IAggregateRoot)note).Version);
                        waitHandle.Set();
                    }
                });
            }
            waitHandle.WaitOne();
        }
        [Test]
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
        [Test]
        public void set_application_message_command_handler_test()
        {
            var command = new SetApplicatonMessageCommand()
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
        [Test]
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
        [Test]
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
        [Test]
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
        [Test]
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
        [Test]
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
        [Test]
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
                IsDomainException = false
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
                IsDomainException = true
            };
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
        }
        [Test]
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

        #region Event Service Tests

        [Test]
        public void create_concurrent_conflict_and_then_update_many_times_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var commandId = ObjectId.GenerateNewStringId();

            //往EventStore直接插入事件，用于模拟并发冲突的情况
            var eventStream = new DomainEventStream(
                commandId,
                aggregateId,
                typeof(TestAggregate).FullName,
                DateTime.Now,
                new IDomainEvent[] { new TestAggregateCreated("Note Title") { AggregateRootId = aggregateId, Version = 1 } },
                null);
            var result = _eventStore.BatchAppendAsync(new DomainEventStream[] { eventStream }).Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
            Assert.AreEqual(aggregateId, result.Data.SuccessAggregateRootIdList[0]);
            _logger.Info("----create_concurrent_conflict_and_then_update_many_times_test, _eventStore.AppendAsync success");

            var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 1).Result;
            Assert.IsNotNull(result2);
            Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);
            _logger.Info("----create_concurrent_conflict_and_then_update_many_times_test, UpdatePublishedVersionAsync success");

            //执行创建聚合根的命令
            var command = new CreateTestAggregateCommand
            {
                Id = commandId,
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            };
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            _logger.Info("----create_concurrent_conflict_and_then_update_many_times_test, _commandService.ExecuteAsync create success");

            var commandList = new List<ICommand>();
            for (var i = 0; i < 50; i++)
            {
                commandList.Add(new ChangeTestAggregateTitleCommand
                {
                    AggregateRootId = aggregateId,
                    Title = "Changed Note Title"
                });
            }

            var waitHandle = new ManualResetEvent(false);
            var count = 0L;
            foreach (var updateCommand in commandList)
            {
                _commandService.ExecuteAsync(updateCommand).ContinueWith(t =>
                {
                    Assert.IsNotNull(t.Result);
                    Assert.AreEqual(AsyncTaskStatus.Success, t.Result.Status);
                    var updateCommandResult = t.Result.Data;
                    Assert.IsNotNull(updateCommandResult);
                    Assert.AreEqual(CommandStatus.Success, updateCommandResult.Status);
                    var totalCount = Interlocked.Increment(ref count);
                    _logger.InfoFormat("----create_concurrent_conflict_and_then_update_many_times_test, updateCommand finished, count: {0}", totalCount);
                    if (totalCount == commandList.Count)
                    {
                        waitHandle.Set();
                    }
                });
            }
            waitHandle.WaitOne();
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual(commandList.Count + 1, ((IAggregateRoot)note).Version);
        }
        [Test]
        public void create_concurrent_conflict_and_then_update_many_times_test2()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var commandId = ObjectId.GenerateNewStringId();

            //往EventStore直接插入事件，用于模拟并发冲突的情况
            var eventStream = new DomainEventStream(
                commandId,
                aggregateId,
                typeof(TestAggregate).FullName,
                DateTime.Now,
                new IDomainEvent[] { new TestAggregateCreated("Note Title") { AggregateRootId = aggregateId, Version = 1 } },
                null);
            var result = _eventStore.BatchAppendAsync(new DomainEventStream[] { eventStream }).Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
            Assert.AreEqual(aggregateId, result.Data.SuccessAggregateRootIdList[0]);

            var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 1).Result;
            Assert.IsNotNull(result2);
            Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);

            var commandList = new List<ICommand>();
            commandList.Add(new CreateTestAggregateCommand
            {
                Id = commandId,
                AggregateRootId = aggregateId,
                Title = "Sample Note"
            });
            for (var i = 0; i < 50; i++)
            {
                commandList.Add(new ChangeTestAggregateTitleCommand
                {
                    AggregateRootId = aggregateId,
                    Title = "Changed Note Title"
                });
            }

            var waitHandle = new ManualResetEvent(false);
            var count = 0L;
            var createCommandSuccess = false;
            foreach (var updateCommand in commandList)
            {
                _commandService.ExecuteAsync(updateCommand).ContinueWith(t =>
                {
                    Assert.IsNotNull(t.Result);
                    Assert.AreEqual(AsyncTaskStatus.Success, t.Result.Status);
                    var commandResult = t.Result.Data;
                    Assert.IsNotNull(commandResult);
                    Assert.AreEqual(CommandStatus.Success, commandResult.Status);
                    if (commandResult.CommandId != commandId)
                    {
                        var totalCount = Interlocked.Increment(ref count);
                        if (totalCount == commandList.Count - 1)
                        {
                            waitHandle.Set();
                        }
                    }
                    else
                    {
                        createCommandSuccess = true;
                    }
                });
            }
            waitHandle.WaitOne();
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual(true, createCommandSuccess);
            Assert.AreEqual(commandList.Count, ((IAggregateRoot)note).Version);
        }
        [Test]
        public void update_concurrent_conflict_test()
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
            var note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //往EventStore直接插入事件，用于模拟并发冲突的情况
            var eventStream = new DomainEventStream(
                ObjectId.GenerateNewStringId(),
                aggregateId,
                typeof(TestAggregate).FullName,
                DateTime.Now,
                new IDomainEvent[] { new TestAggregateTitleChanged("Changed Title") { AggregateRootId = aggregateId, Version = 2 } },
                null);
            var result = _eventStore.BatchAppendAsync(new DomainEventStream[] { eventStream }).Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
            Assert.AreEqual(aggregateId, result.Data.SuccessAggregateRootIdList[0]);

            var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 2).Result;
            Assert.IsNotNull(result2);
            Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);

            var commandList = new List<ICommand>();
            for (var i = 0; i < 50; i++)
            {
                commandList.Add(new ChangeTestAggregateTitleCommand
                {
                    AggregateRootId = aggregateId,
                    Title = "Changed Note2"
                });
            }

            var waitHandle = new ManualResetEvent(false);
            var count = 0L;
            foreach (var updateCommand in commandList)
            {
                _commandService.ExecuteAsync(updateCommand).ContinueWith(t =>
                {
                    Assert.IsNotNull(t.Result);
                    Assert.AreEqual(AsyncTaskStatus.Success, t.Result.Status);
                    var currentCommandResult = t.Result.Data;
                    Assert.IsNotNull(currentCommandResult);
                    Assert.AreEqual(CommandStatus.Success, currentCommandResult.Status);
                    var totalCount = Interlocked.Increment(ref count);
                    if (totalCount == commandList.Count)
                    {
                        waitHandle.Set();
                    }
                });
            }
            waitHandle.WaitOne();
            note = _memoryCache.GetAsync<TestAggregate>(aggregateId).Result;
            Assert.IsNotNull(note);
            Assert.AreEqual(2 + commandList.Count, ((IAggregateRoot)note).Version);
            Assert.AreEqual("Changed Note2", note.Title);
        }

        #endregion

        [Test]
        public void event_handler_priority_test()
        {
            var noteId = ObjectId.GenerateNewStringId();
            var command1 = new CreateTestAggregateCommand { AggregateRootId = noteId, Title = "Sample Title1" };
            var command2 = new TestEventPriorityCommand { AggregateRootId = noteId };
            var commandResult1 = _commandService.ExecuteAsync(command1, CommandReturnType.EventHandled).Result.Data;
            var commandResult2 = _commandService.ExecuteAsync(command2, CommandReturnType.EventHandled).Result.Data;

            Thread.Sleep(3000);

            Assert.AreEqual(CommandStatus.Success, commandResult1.Status);
            Assert.AreEqual(CommandStatus.Success, commandResult2.Status);

            Assert.AreEqual(3, HandlerTypes[1].Count);
            Assert.AreEqual(typeof(Handler3).Name, HandlerTypes[1][0]);
            Assert.AreEqual(typeof(Handler2).Name, HandlerTypes[1][1]);
            Assert.AreEqual(typeof(Handler1).Name, HandlerTypes[1][2]);

            Assert.AreEqual(3, HandlerTypes[2].Count);
            Assert.AreEqual(typeof(Handler122).Name, HandlerTypes[2][0]);
            Assert.AreEqual(typeof(Handler121).Name, HandlerTypes[2][1]);
            Assert.AreEqual(typeof(Handler123).Name, HandlerTypes[2][2]);

            Assert.AreEqual(3, HandlerTypes[3].Count);
            Assert.AreEqual(typeof(Handler1232).Name, HandlerTypes[3][0]);
            Assert.AreEqual(typeof(Handler1231).Name, HandlerTypes[3][1]);
            Assert.AreEqual(typeof(Handler1233).Name, HandlerTypes[3][2]);

            HandlerTypes.Clear();
        }
        [Test]
        public void sequence_domain_event_process_test()
        {
            var processor = ObjectContainer.Resolve<IProcessingEventProcessor>();

            var note = new TestAggregate(ObjectId.GenerateNewStringId(), "initial title");
            var aggregate = note as IAggregateRoot;
            var message1 = CreateMessage(aggregate);

            aggregate.AcceptChanges();
            note.ChangeTitle("title1");
            var message2 = CreateMessage(aggregate);

            aggregate.AcceptChanges();
            note.ChangeTitle("title2");
            var message3 = CreateMessage(aggregate);

            var waitHandle = new ManualResetEvent(false);
            var versionList = new List<int>();

            processor.Process(new ProcessingEvent(message1, new DomainEventStreamProcessContext(message1, waitHandle, versionList)));
            processor.Process(new ProcessingEvent(message3, new DomainEventStreamProcessContext(message3, waitHandle, versionList)));
            processor.Process(new ProcessingEvent(message2, new DomainEventStreamProcessContext(message2, waitHandle, versionList)));

            waitHandle.WaitOne();

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i + 1, versionList[i]);
            }
        }

        private DomainEventStreamMessage CreateMessage(IAggregateRoot aggregateRoot)
        {
            return new DomainEventStreamMessage(
                ObjectId.GenerateNewStringId(),
                aggregateRoot.UniqueId,
                aggregateRoot.Version + 1,
                aggregateRoot.GetType().FullName,
                aggregateRoot.GetChanges(),
                new Dictionary<string, string>());
        }
        class DomainEventStreamProcessContext : IEventProcessContext
        {
            private DomainEventStreamMessage _domainEventStreamMessage;
            private ManualResetEvent _waitHandle;
            private IList<int> _versionList;

            public DomainEventStreamProcessContext(DomainEventStreamMessage domainEventStreamMessage, ManualResetEvent waitHandle, IList<int> versionList)
            {
                _domainEventStreamMessage = domainEventStreamMessage;
                _waitHandle = waitHandle;
                _versionList = versionList;
            }
            public void NotifyEventProcessed()
            {
                _versionList.Add(_domainEventStreamMessage.Version);
                if (_domainEventStreamMessage.Version == 3)
                {
                    _waitHandle.Set();
                }
            }
        }
    }
}
