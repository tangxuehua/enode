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
            context.Add(new Note(command.AggregateRootId, command.Title));
            return Task.CompletedTask;
        }
    }
}
