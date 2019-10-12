using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using NUnit.Framework;

namespace ENode.Tests
{
    [TestFixture]
    public class PublishedVersionStoreFailedTest : BaseTest
    {
        [OneTimeSetUp]
        public void ClassInitialize()
        {
            Initialize(useMockPublishedVersionStore: true);
        }

        [Test]
        public void published_version_store_failed_test()
        {
            var mockPublishedVersionStore = _publishedVersionStore as MockPublishedVersionStore;
            var command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockPublishedVersionStore.SetExpectFailedCount(FailedType.UnKnownException, 5);
            var commandResult = _commandService.ExecuteAsync(command, CommandReturnType.EventHandled).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockPublishedVersionStore.Reset();

            command = new CreateTestAggregateCommand
            {
                AggregateRootId = ObjectId.GenerateNewStringId(),
                Title = "Sample Note"
            };
            mockPublishedVersionStore.SetExpectFailedCount(FailedType.IOException, 5);
            commandResult = _commandService.ExecuteAsync(command, CommandReturnType.EventHandled).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Success, commandResult.Status);
            mockPublishedVersionStore.Reset();
        }
    }
}
