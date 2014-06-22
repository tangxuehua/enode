using ECommon.Components;
using ECommon.Logging;
using ENode.Eventing;
using NoteSample.DomainEvents;

namespace NoteSample.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreatedEvent>, IEventHandler<NoteTitleChangedEvent>
    {
        private ILogger _logger;

        public NoteEventHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
        }

        public void Handle(IEventContext context, NoteCreatedEvent evnt)
        {
            _logger.InfoFormat("Note Created, Title：{0}, Version: {1}", evnt.Title, evnt.Version);
        }
        public void Handle(IEventContext context, NoteTitleChangedEvent evnt)
        {
            _logger.InfoFormat("Note Updated, Title：{0}, Version: {1}", evnt.Title, evnt.Version);
        }
    }
}
