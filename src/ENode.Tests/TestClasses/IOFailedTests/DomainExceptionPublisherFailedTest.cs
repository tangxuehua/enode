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
            var commandResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockDomainExceptionPublisher)_domainExceptionPublisher).Reset();

            ((MockDomainExceptionPublisher)_domainExceptionPublisher).SetExpectFailedCount(FailedType.IOException, 5);
            commandResult = _commandService.ExecuteAsync(command1).Result;
            Assert.IsNotNull(commandResult);
            Assert.AreEqual(CommandStatus.Failed, commandResult.Status);
            ((MockDomainExceptionPublisher)_domainExceptionPublisher).Reset();
        }
    }
}
