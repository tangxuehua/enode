using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class PublishableExceptionPublisherFailedTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context, useMockPublishableExceptionPublisher: true);
        }

        [TestMethod]
        public void publishable_exception_publisher_throw_exception_test()
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
                PublishableException = true
            };
            ((MockPublishableExceptionPublisher)_publishableExceptionPublisher).SetExpectFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockPublishableExceptionPublisher)_publishableExceptionPublisher).Reset();

            ((MockPublishableExceptionPublisher)_publishableExceptionPublisher).SetExpectFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockPublishableExceptionPublisher)_publishableExceptionPublisher).Reset();

            ((MockPublishableExceptionPublisher)_publishableExceptionPublisher).SetExpectFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockPublishableExceptionPublisher)_publishableExceptionPublisher).Reset();
        }
    }
}
