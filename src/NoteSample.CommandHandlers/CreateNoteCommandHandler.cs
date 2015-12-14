using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;

namespace NoteSample.CommandHandlers
{
    public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand>
    {
        public void Handle(ICommandContext context, CreateNoteCommand command)
        {
            context.Add(new Note(command.AggregateRootId, command.Title));
        }
    }
}
