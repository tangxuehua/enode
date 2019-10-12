using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using NUnit.Framework;

namespace ENode.Tests
{
    [TestFixture]
    public class EventStoreFailedTest : BaseTest
    {
        [OneTimeSetUp]
        public void ClassInitialize()
        {
            Initialize(useMockEventStore: true);
        }

        [Test]
        public void event_store_failed_test()
        {
            var mockEventStore = _eventStore as MockEventStore;
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockEventStore.SetExpectFailedCount(FailedType.UnKnownException, 5);
            var commandResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockEventStore.Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockEventStore.SetExpectFailedCount(FailedType.IOException, 5);
            commandResult = _commandService.ExecuteAsync(command).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockEventStore.Reset();
        }
    }
}
