using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class PublishedVersionStoreFailedTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context, useMockPublishedVersionStore: true);
        }

        [TestMethod]
        public void published_version_store_failed_test()
        {
            var mockPublishedVersionStore = _publishedVersionStore as MockPublishedVersionStore;
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockPublishedVersionStore.SetExpectFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(command, CommandReturnType.EventHandled).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockPublishedVersionStore.Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockPublishedVersionStore.SetExpectFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(command, CommandReturnType.EventHandled).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockPublishedVersionStore.Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockPublishedVersionStore.SetExpectFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(command, CommandReturnType.EventHandled).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockPublishedVersionStore.Reset();
        }
    }
}
