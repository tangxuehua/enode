using ECommon.IoC;
using ENode.Commanding;
using DistributeEventStoreSample.Client.Commands;
using DistributeEventStoreSample.Client.Domain;

namespace DistributeEventStoreSample.Client.CommandHandlers
{
    [Component]
    public class ChangeNoteTitleCommandHandler : ICommandHandler<ChangeNoteTitleCommand>
    {
        public void Handle(ICommandContext context, ChangeNoteTitleCommand command)
        {
            context.Get<Note>(command.AggregateRootId).ChangeTitle(command.Title);
        }
    }
}
