using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;
using System.Threading.Tasks;

namespace NoteSample.CommandHandlers
{
    public class ChangeNoteTitleCommandHandler : ICommandHandler<ChangeNoteTitleCommand>
    {
        public async Task HandleAsync(ICommandContext context, ChangeNoteTitleCommand command)
        {
            var note = await context.GetAsync<Note>(command.AggregateRootId);
            note.ChangeTitle(command.Title);
        }
    }
}
