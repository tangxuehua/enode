using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;

namespace NoteSample.CommandHandlers
{
    public class ChangeNoteTitleCommandHandler : ICommandHandler<ChangeNoteTitleCommand>
    {
        public void Handle(ICommandContext context, ChangeNoteTitleCommand command)
        {
            context.Get<Note>(command.AggregateRootId).ChangeTitle(command.Title);
        }
    }
}
