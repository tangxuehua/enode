using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.Tests
{
    [TestClass]
    public class EventPublisherFailedTest : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context, useMockDomainEventPublisher: true);
        }

        [TestMethod]
        public void event_publisher_failed_test()
        {
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            ((MockDomainEventPublisher)_domainEventPublisher).SetExpectFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockDomainEventPublisher)_domainEventPublisher).Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            ((MockDomainEventPublisher)_domainEventPublisher).SetExpectFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockDomainEventPublisher)_domainEventPublisher).Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            ((MockDomainEventPublisher)_domainEventPublisher).SetExpectFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockDomainEventPublisher)_domainEventPublisher).Reset();
        }
    }
}
