using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using NUnit.Framework;

namespace ENode.Tests
{
    [TestFixture]
    public class EventPublisherFailedTest : BaseTest
    {
        [OneTimeSetUp]
        public void ClassInitialize()
        {
            Initialize(useMockDomainEventPublisher: true);
        }

        [Test]
        public void event_publisher_failed_test()
        {
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            ((MockDomainEventPublisher)_domainEventPublisher).SetExpectFailedCount(FailedType.UnKnownException, 5);
            var commandResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockDomainEventPublisher)_domainEventPublisher).Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            ((MockDomainEventPublisher)_domainEventPublisher).SetExpectFailedCount(FailedType.IOException, 5);
            commandResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            ((MockDomainEventPublisher)_domainEventPublisher).Reset();
        }
    }
}
