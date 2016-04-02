using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class ApplicationMessagePublisherFailedTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context, useMockApplicationMessagePublisher: true);
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
            Cleanup();
        }

        [TestMethod]
        public void async_command_application_message_publish_failed_test()
        {
            ((MockApplicationMessagePublisher)_applicationMessagePublisher).SetExpectFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                ShouldGenerateApplicationMessage = true
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockApplicationMessagePublisher)_applicationMessagePublisher).Reset();

            ((MockApplicationMessagePublisher)_applicationMessagePublisher).SetExpectFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                ShouldGenerateApplicationMessage = true
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockApplicationMessagePublisher)_applicationMessagePublisher).Reset();

            ((MockApplicationMessagePublisher)_applicationMessagePublisher).SetExpectFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(new AsyncHandlerCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                ShouldGenerateApplicationMessage = true
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockApplicationMessagePublisher)_applicationMessagePublisher).Reset();
        }
    }
}
