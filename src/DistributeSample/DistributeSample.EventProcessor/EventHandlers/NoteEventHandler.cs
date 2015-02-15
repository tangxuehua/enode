using ECommon.Components;
using ECommon.Logging;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace DistributeSample.EventProcessor.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreated>
    {
        private ILogger _logger;

        public NoteEventHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
        }

        public void Handle(IHandlingContext context, NoteCreated evnt)
        {
            _logger.InfoFormat("Note created, Title：{0}", evnt.Title);
        }
    }
}
