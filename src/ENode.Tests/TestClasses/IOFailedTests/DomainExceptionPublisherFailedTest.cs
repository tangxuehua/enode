using ECommon.IO;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Tests.Commands;
using NUnit.Framework;

namespace ENode.Tests
{
    [TestFixture]
    public class DomainExceptionPublisherFailedTest : BaseTest
    {
        [OneTimeSetUp]
        public void ClassInitialize()
        {
            Initialize(useMockDomainExceptionPublisher: true);
        }

        [Test]
        public void domain_exception_publisher_throw_exception_test()
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
                IsDomainException = true
            };
            ((MockDomainExceptionPublisher)_domainExceptionPublisher).SetExpectFailedCount(FailedType.UnKnownException, 5);
            var asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            var commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockDomainExceptionPublisher)_domainExceptionPublisher).Reset();

            ((MockDomainExceptionPublisher)_domainExceptionPublisher).SetExpectFailedCount(FailedType.IOException, 5);
            asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockDomainExceptionPublisher)_domainExceptionPublisher).Reset();

            ((MockDomainExceptionPublisher)_domainExceptionPublisher).SetExpectFailedCount(FailedType.TaskIOException, 5);
            asyncResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(asyncResult);
            Assert.AreEqual(AsyncTaskStatus.Success, asyncResult.Status);
            commandResult = asyncResult.Data;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockDomainExceptionPublisher)_domainExceptionPublisher).Reset();
        }
    }
}
