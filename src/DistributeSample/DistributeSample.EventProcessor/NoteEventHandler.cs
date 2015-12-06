using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.IO;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace DistributeSample.EventProcessor
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
            _logger.InfoFormat("Note denormalizered, title：{0}", evnt.Title);
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
