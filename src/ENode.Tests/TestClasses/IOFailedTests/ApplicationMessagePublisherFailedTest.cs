using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using NUnit.Framework;

namespace ENode.Tests
{
    [TestFixture]
    public class ApplicationMessagePublisherFailedTest : BaseTest
    {
        [OneTimeSetUp]
        public void ClassInitialize()
        {
            Initialize(useMockApplicationMessagePublisher: true);
        }

        [Test]
        public void command_application_message_publish_failed_test()
        {
            ((MockApplicationMessagePublisher)_applicationMessagePublisher).SetExpectFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(new SetApplicatonMessageCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockApplicationMessagePublisher)_applicationMessagePublisher).Reset();

            ((MockApplicationMessagePublisher)_applicationMessagePublisher).SetExpectFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(new SetApplicatonMessageCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
            }).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockApplicationMessagePublisher)_applicationMessagePublisher).Reset();

            ((MockApplicationMessagePublisher)_applicationMessagePublisher).SetExpectFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(new SetApplicatonMessageCommand()
            {
                AggregateRootId = ObjectId.GenerateNewStringId()
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
