using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.IO;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace DistributeSample.EventProcessor.EventHandlers
{
    [Component]
    public class NoteEventHandler : IMessageHandler<NoteCreated>
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
    }
}
