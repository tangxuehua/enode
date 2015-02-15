using ECommon.Components;
using ECommon.Logging;
using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;

namespace NoteSample.CommandHandlers
{
    [Component]
    public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand>
    {
        private ILogger _logger;

        public CreateNoteCommandHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(CreateNoteCommandHandler).Name);
        }

        public void Handle(ICommandContext context, CreateNoteCommand command)
        {
            context.Add(new Note(command.AggregateRootId, command.Title));
            _logger.InfoFormat("Handled {0}, Note Title:{1}", typeof(CreateNoteCommand).Name, command.Title);
        }
    }
}
