using System.Threading.Tasks;
using ECommon.IO;
using ECommon.Logging;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace NoteSample.QuickStart
{
    public class NoteEventHandler : IMessageHandler<NoteCreated>, IMessageHandler<NoteTitleChanged>
    {
        private ILogger _logger;

        public NoteEventHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
        }

        public Task<AsyncTaskResult> HandleAsync(NoteCreated evnt)
        {
            _logger.InfoFormat("Note denormalizered, title：{0}, Version: {1}", evnt.Title, evnt.Version);
            return Task.FromResult(AsyncTaskResult.Success);
        }
        public Task<AsyncTaskResult> HandleAsync(NoteTitleChanged evnt)
        {
            _logger.InfoFormat("Note denormalizered, title：{0}, Version: {1}", evnt.Title, evnt.Version);
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
