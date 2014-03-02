using ECommon.IoC;
using ENode.Commanding;
using DistributeEventStoreSample.Client.Commands;
using DistributeEventStoreSample.Client.Domain;

namespace DistributeEventStoreSample.Client.CommandHandlers
{
    [Component]
    public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand>
    {
        public void Handle(ICommandContext context, CreateNoteCommand command)
        {
            context.Add(new Note(command.AggregateRootId, command.Title));
        }
    }
}
