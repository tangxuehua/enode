using System.Threading.Tasks;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Messaging;
using NoteSample.Domain;

namespace NoteSample.QuickStart
{
    public class NoteEventHandler :
        IMessageHandler<NoteCreated>,
        IMessageHandler<NoteTitleChanged>
    {
        private ILogger _logger;
        private IJsonSerializer _jsonSerializer;

        public NoteEventHandler(ILoggerFactory loggerFactory, IJsonSerializer jsonSerializer)
        {
            _logger = loggerFactory.Create(typeof(NoteEventHandler).Name);
            _jsonSerializer = jsonSerializer;
        }

        public Task HandleAsync(NoteCreated evnt)
        {
            _logger.InfoFormat("Note created, title：{0}, version: {1}, items: {2}", evnt.Title, evnt.Version, _jsonSerializer.Serialize(evnt.Items));
            return Task.CompletedTask;
        }
        public Task HandleAsync(NoteTitleChanged evnt)
        {
            _logger.InfoFormat("Note title changed, title：{0}, version: {1}, items: {2}", evnt.Title, evnt.Version, _jsonSerializer.Serialize(evnt.Items));
            return Task.CompletedTask;
        }
    }
}
