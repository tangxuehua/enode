using System;
using DistributeSample.Events;
using ECommon.Components;
using ECommon.Logging;
using ENode.Eventing;

namespace DistributeSample.EventProcessor.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreatedEvent>
    {
        private ILogger _logger;

        public NoteEventHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
        }

        public void Handle(IEventContext context, NoteCreatedEvent evnt)
        {
            _logger.InfoFormat("Note created, Title：{0}", evnt.Title);
        }
    }
}
