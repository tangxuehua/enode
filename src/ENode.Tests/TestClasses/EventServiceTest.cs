using System;
using System.Collections.Generic;
using System.Threading;
using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Tests.Commands;
using ENode.Tests.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class EventServiceTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context);
        }

        [TestMethod]
        public void create_concurrent_conflict_and_then_update_many_times_test()
        {
            var aggregateId = ObjectId.GenerateNewStringId();
            var commandId = ObjectId.GenerateNewStringId();

            //往EventStore直接插入事件，用于模拟并发冲突的情况
            var eventStream = new DomainEventStream(
                commandId,
                aggregateId,
                typeof(TestAggregate).FullName,
                1,
                DateTime.Now,
                new IDomainEvent[] { new TestAggregateTitleChanged("Note Title") { AggregateRootId = aggregateId, Version = 1 } },
                null);
            var result = _eventStore.AppendAsync(eventStream).Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
            Assert.AreEqual(EventAppendResult.Success, result.Data);

            var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 1).Result;
            Assert.IsNotNull(result2);
            Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);

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

            var commandList = new List<ICommand>();
            for (var i = 0; i < 5000; i++)
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
                    if (totalCount == commandList.Count)
                    {
                        waitHandle.Set();
                    }
                });
            }
            waitHandle.WaitOne();
            var note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual(commandList.Count + 1, ((IAggregateRoot)note).Version);
        }
        [TestMethod]
        public void create_concurrent_conflict_and_then_update_many_times_not_enable_batch_insert_test()
        {
            _eventStore.SupportBatchAppendEvent = false;

            try
            {
                var aggregateId = ObjectId.GenerateNewStringId();
                var commandId = ObjectId.GenerateNewStringId();

                //往EventStore直接插入事件，用于模拟并发冲突的情况
                var eventStream = new DomainEventStream(
                    commandId,
                    aggregateId,
                    typeof(TestAggregate).FullName,
                    1,
                    DateTime.Now,
                    new IDomainEvent[] { new TestAggregateTitleChanged("Note Title") { AggregateRootId = aggregateId, Version = 1 } },
                    null);
                var result = _eventStore.AppendAsync(eventStream).Result;
                Assert.IsNotNull(result);
                Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
                Assert.AreEqual(EventAppendResult.Success, result.Data);

                var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 1).Result;
                Assert.IsNotNull(result2);
                Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);

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

                var commandList = new List<ICommand>();
                for (var i = 0; i < 5000; i++)
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
                        if (totalCount == commandList.Count)
                        {
                            waitHandle.Set();
                        }
                    });
                }
                waitHandle.WaitOne();
                var note = _memoryCache.Get<TestAggregate>(aggregateId);
                Assert.IsNotNull(note);
                Assert.AreEqual(commandList.Count + 1, ((IAggregateRoot)note).Version);
            }
            finally
            {
                _eventStore.SupportBatchAppendEvent = true;
            }
        }

        [TestMethod]
        public void create_concurrent_conflict_not_enable_batch_insert_test()
        {
            _eventStore.SupportBatchAppendEvent = false;

            try
            {
                var aggregateId = ObjectId.GenerateNewStringId();
                var commandId = ObjectId.GenerateNewStringId();

                //往EventStore直接插入事件，用于模拟并发冲突的情况
                var eventStream = new DomainEventStream(
                    commandId,
                    aggregateId,
                    typeof(TestAggregate).FullName,
                    1,
                    DateTime.Now,
                    new IDomainEvent[] { new TestAggregateTitleChanged("Note Title") { AggregateRootId = aggregateId, Version = 1 } },
                    null);
                var result = _eventStore.AppendAsync(eventStream).Result;
                Assert.IsNotNull(result);
                Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
                Assert.AreEqual(EventAppendResult.Success, result.Data);
                var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 1).Result;
                Assert.IsNotNull(result2);
                Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);

                //执行创建聚合根的命令
                var command = new CreateTestAggregateCommand
                {
                    AggregateRootId = aggregateId,
                    Title = "Sample Note"
                };
                var asyncResult = _commandService.ExecuteAsync(command).Result;
                Assert.IsNotNull(asyncResult);
                Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
                var commandResult = asyncResult.Data;
                Assert.IsNotNull(commandResult);
                Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
                var note = _memoryCache.Get<TestAggregate>(aggregateId);
                Assert.IsNotNull(note);
                Assert.AreEqual("Note Title", note.Title);
                Assert.AreEqual(1, ((IAggregateRoot)note).Version);

                //执行创建聚合根的命令
                command = new CreateTestAggregateCommand
                {
                    Id = commandId,
                    AggregateRootId = aggregateId,
                    Title = "Sample Note"
                };
                asyncResult = _commandService.ExecuteAsync(command).Result;
                Assert.IsNotNull(asyncResult);
                Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
                commandResult = asyncResult.Data;
                Assert.IsNotNull(commandResult);
                Assert.AreEqual(CommandStatus.Success, commandResult.Status);
                note = _memoryCache.Get<TestAggregate>(aggregateId);
                Assert.IsNotNull(note);
                Assert.AreEqual("Note Title", note.Title);
                Assert.AreEqual(1, ((IAggregateRoot)note).Version);
            }
            finally
            {
                _eventStore.SupportBatchAppendEvent = true;
            }
        }

        [TestMethod]
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
            var note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual("Sample Note", note.Title);
            Assert.AreEqual(1, ((IAggregateRoot)note).Version);

            //往EventStore直接插入事件，用于模拟并发冲突的情况
            var eventStream = new DomainEventStream(
                ObjectId.GenerateNewStringId(),
                aggregateId,
                typeof(TestAggregate).FullName,
                2,
                DateTime.Now,
                new IDomainEvent[] { new TestAggregateTitleChanged("Changed Title") { AggregateRootId = aggregateId, Version = 2 } },
                null);
            var result = _eventStore.AppendAsync(eventStream).Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
            Assert.AreEqual(EventAppendResult.Success, result.Data);

            var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 2).Result;
            Assert.IsNotNull(result2);
            Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);

            //执行修改聚合根的命令
            var command2 = new ChangeTestAggregateTitleCommand
            {
                AggregateRootId = aggregateId,
                Title = "Changed Note2"
            };
            asyncResult = _commandService.ExecuteAsync(command2).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            note = _memoryCache.Get<TestAggregate>(aggregateId);
            Assert.IsNotNull(note);
            Assert.AreEqual(3, ((IAggregateRoot)note).Version);
            Assert.AreEqual("Changed Note2", note.Title);
        }
        [TestMethod]
        public void update_concurrent_conflict_not_enable_batch_insert_test()
        {
            _eventStore.SupportBatchAppendEvent = false;

            try
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

                //往EventStore直接插入事件，用于模拟并发冲突的情况
                var eventStream = new DomainEventStream(
                    ObjectId.GenerateNewStringId(),
                    aggregateId,
                    typeof(TestAggregate).FullName,
                    2,
                    DateTime.Now,
                    new IDomainEvent[] { new TestAggregateTitleChanged("Changed Title") { AggregateRootId = aggregateId, Version = 2 } },
                    null);
                var result = _eventStore.AppendAsync(eventStream).Result;
                Assert.IsNotNull(result);
                Assert.AreEqual(AsyncTaskStatus.Success, result.Status);
                Assert.AreEqual(EventAppendResult.Success, result.Data);

                var result2 = _publishedVersionStore.UpdatePublishedVersionAsync("DefaultEventProcessor", typeof(TestAggregate).FullName, aggregateId, 2).Result;
                Assert.IsNotNull(result2);
                Assert.AreEqual(AsyncTaskStatus.Success, result2.Status);

                //执行修改聚合根的命令
                var command2 = new ChangeTestAggregateTitleCommand
                {
                    AggregateRootId = aggregateId,
                    Title = "Changed Note2"
                };
                asyncResult = _commandService.ExecuteAsync(command2).Result;
                Assert.IsNotNull(asyncResult);
                Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
                commandResult = asyncResult.Data;
                Assert.IsNotNull(commandResult);
                Assert.AreEqual(CommandStatus.Success, commandResult.Status);
                note = _memoryCache.Get<TestAggregate>(aggregateId);
                Assert.IsNotNull(note);
                Assert.AreEqual(3, ((IAggregateRoot)note).Version);
                Assert.AreEqual("Changed Note2", note.Title);
            }
            finally
            {
                _eventStore.SupportBatchAppendEvent = true;
            }
        }
    }
}
