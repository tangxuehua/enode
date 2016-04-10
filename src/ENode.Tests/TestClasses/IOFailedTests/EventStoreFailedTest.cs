using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class EventStoreFailedTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context, useMockEventStore: true);
        }

        [TestMethod]
        public void event_store_failed_test()
        {
            var mockEventStore = _eventStore as MockEventStore;
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockEventStore.SetExpectFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockEventStore.Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockEventStore.SetExpectFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockEventStore.Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockEventStore.SetExpectFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockEventStore.Reset();
        }
    }
}
