using System.Collections.Generic;
using System.Threading;
using ECommon.Components;
using ECommon.Utilities;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ENode.UnitTests
{
    [TestClass]
    public class DomainEventSequenceTest : BaseTest
    {
        private static ManualResetEvent _waitHandle = new ManualResetEvent(false);
        private static IList<int> _versionList = new List<int>();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Initialize(context);
        }

        [TestMethod]
        public void sequence_domain_event_process_test()
        {
            var processor = ObjectContainer.Resolve<IMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>>();

            var note = new Note(ObjectId.GenerateNewStringId(), "initial title");
            var aggregate = note as IAggregateRoot;
            var message1 = CreateMessage(aggregate);

            aggregate.AcceptChanges(1);
            note.ChangeTitle("title1");
            var message2 = CreateMessage(aggregate);

            aggregate.AcceptChanges(2);
            note.ChangeTitle("title2");
            var message3 = CreateMessage(aggregate);

            processor.Process(new ProcessingDomainEventStreamMessage(message1, new DomainEventStreamProcessContext(message1)));
            processor.Process(new ProcessingDomainEventStreamMessage(message3, new DomainEventStreamProcessContext(message3)));
            processor.Process(new ProcessingDomainEventStreamMessage(message2, new DomainEventStreamProcessContext(message2)));

            _waitHandle.WaitOne();

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i + 1, _versionList[i]);
            }
        }

        private DomainEventStreamMessage CreateMessage(IAggregateRoot aggregateRoot)
        {
            return new DomainEventStreamMessage(
                ObjectId.GenerateNewStringId(),
                aggregateRoot.UniqueId,
                aggregateRoot.Version + 1,
                aggregateRoot.GetType().FullName,
                aggregateRoot.GetChanges(),
                new Dictionary<string, string>());
        }
        class DomainEventStreamProcessContext : IMessageProcessContext
        {
            private DomainEventStreamMessage _domainEventStreamMessage;

            public DomainEventStreamProcessContext(DomainEventStreamMessage domainEventStreamMessage)
            {
                _domainEventStreamMessage = domainEventStreamMessage;
            }
            public void NotifyMessageProcessed()
            {
                _versionList.Add(_domainEventStreamMessage.Version);
                if (_domainEventStreamMessage.Version == 3)
                {
                    _waitHandle.Set();
                }
            }
        }
    }
}
