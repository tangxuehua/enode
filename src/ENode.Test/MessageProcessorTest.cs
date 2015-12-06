using System;
using System.Collections.Generic;
using System.Threading;
using ECommon.Components;
using ECommon.Utilities;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.Domain;
using NUnit.Framework;

namespace ENode.Test
{
    [TestFixture]
    public class MessageProcessorTest : TestBase
    {
        private static ManualResetEvent _waitHandle = new ManualResetEvent(false);

        [Test]
        public void test_sequence_domain_event_process()
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
                _logger.InfoFormat("Domain event processed, aggregateId:{0}, version:{1}", _domainEventStreamMessage.AggregateRootId, _domainEventStreamMessage.Version);
                if (_domainEventStreamMessage.Version == 3)
                {
                    _waitHandle.Set();
                }
            }
        }
    }
}
