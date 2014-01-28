using DistributeSample.CommandProcessor.Domain;
using DistributeSample.Commands;
using ECommon.IoC;
using ENode.Commanding;

namespace DistributeSample.CommandProcessor.CommandHandlers
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
