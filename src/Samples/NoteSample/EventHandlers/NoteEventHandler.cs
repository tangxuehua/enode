using System.Threading;
using ECommon.Components;
using ECommon.Logging;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.DomainEvents;

namespace NoteSample.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreated>, IEventHandler<NoteTitleChanged>
    {
        private int _count;
        private ILogger _logger;

        public NoteEventHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
        }

        public void Handle(IHandlingContext context, NoteCreated evnt)
        {
            if (Interlocked.Increment(ref _count) % 100 == 0)
            {
                _logger.InfoFormat("Note Created, Id:{0}, Title：{1}, Version: {2}", evnt.AggregateRootId, evnt.Title, evnt.Version);
            }
        }
        public void Handle(IHandlingContext context, NoteTitleChanged evnt)
        {
            _logger.InfoFormat("Note Updated, Id:{0}, Title：{1}, Version: {2}", evnt.AggregateRootId, evnt.Title, evnt.Version);
        }
    }
}
