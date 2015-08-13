using ECommon.Components;
using ENode.Commanding;
using NoteSample.Commands;
using NoteSample.Domain;

namespace NoteSample.CommandHandlers
{
    [Component]
    public class TestEventsCommandHandler : ICommandHandler<TestEventsCommand>
    {
        public void Handle(ICommandContext context, TestEventsCommand command)
        {
            context.Get<Note>(command.AggregateRootId).TestEvents();
        }
    }
}
