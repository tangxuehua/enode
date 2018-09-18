using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;
using System.Threading.Tasks;

namespace NoteSample.CommandHandlers
{
    public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand>
    {
        public Task HandleAsync(ICommandContext context, CreateNoteCommand command)
        {
            return context.AddAsync(new Note(command.AggregateRootId, command.Title));
        }
    }
}
