using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace NoteSample.QuickStart.EventHandlers
{
    [Component]
    public class NoteEventHandler : IMessageHandler<NoteCreated>, IMessageHandler<NoteTitleChanged>
    {
        private ILogger _logger;

        public NoteEventHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(NoteCreated evnt)
        {
            _logger.InfoFormat("Note Created, Id:{0}, Title：{1}, Version: {2}", evnt.AggregateRootId, evnt.Title, evnt.Version);
            return Task.FromResult(AsyncTaskResult.Success);
        }
        public Task<AsyncTaskResult> HandleAsync(NoteTitleChanged evnt)
        {
            _logger.InfoFormat("Note Updated, Id:{0}, Title：{1}, Version: {2}", evnt.AggregateRootId, evnt.Title, evnt.Version);
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
