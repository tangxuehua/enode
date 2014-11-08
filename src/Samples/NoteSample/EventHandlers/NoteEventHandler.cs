using ECommon.Components;
using ECommon.Logging;
using ENode.Eventing;
using NoteSample.DomainEvents;

namespace NoteSample.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreated>, IEventHandler<NoteTitleChanged>
    {
        private ILogger _logger;

        public NoteEventHandler(IEventPublisher eventPublisher, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
        }

        public void Handle(IEventContext context, NoteCreated evnt)
        {
            _logger.InfoFormat("Note Created, Id:{0}, Title：{1}, Version: {2}", evnt.AggregateRootId, evnt.Title, evnt.Version);
        }
        public void Handle(IEventContext context, NoteTitleChanged evnt)
        {
            _logger.InfoFormat("Note Updated, Id:{0}, Title：{1}, Version: {2}", evnt.AggregateRootId, evnt.Title, evnt.Version);
        }
    }
}
