using ENode.Commanding;
using ENode.Infrastructure;
using NoteSample.Commands;
using NoteSample.Domain;

namespace NoteSample.CommandHandlers {
    [Component]
    public class ChangeNoteTitleCommandHandler : ICommandHandler<ChangeNoteTitle> {
        public void Handle(ICommandContext context, ChangeNoteTitle command) {
            context.Get<Note>(command.NoteId).ChangeTitle(command.Title);
        }
    }
}
