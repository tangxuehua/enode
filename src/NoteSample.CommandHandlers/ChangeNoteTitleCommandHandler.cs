using ECommon.Components;
using ECommon.Logging;
using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;

namespace NoteSample.CommandHandlers
{
    [Component]
    public class ChangeNoteTitleCommandHandler : ICommandHandler<ChangeNoteTitleCommand>
    {
        private ILogger _logger;

        public ChangeNoteTitleCommandHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType().Name);
        }

        public void Handle(ICommandContext context, ChangeNoteTitleCommand command)
        {
            context.Get<Note>(command.AggregateRootId).ChangeTitle(command.Title);
            _logger.InfoFormat("Handled {0}, Note Title:{1}", typeof(ChangeNoteTitleCommand).Name, command.Title);
        }
    }
}
