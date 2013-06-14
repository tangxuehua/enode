using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;

namespace NoteSample.CommandHandlers
{
    public class ChangeNoteTitleCommandHandler : ICommandHandler<ChangeNoteTitle>
    {
        public void Handle(ICommandContext context, ChangeNoteTitle command)
        {
            context.Get<Note>(command.NoteId).ChangeTitle(command.Title);
        }
    }
}
