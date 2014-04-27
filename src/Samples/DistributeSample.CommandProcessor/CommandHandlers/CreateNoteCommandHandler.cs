using System;
using DistributeSample.CommandProcessor.Domain;
using DistributeSample.Commands;
using ECommon.Components;
using ENode.Commanding;

namespace DistributeSample.CommandProcessor.CommandHandlers
{
    [Component]
    public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand>
    {
        public void Handle(ICommandContext context, CreateNoteCommand command)
        {
            context.Add(new Note(command.AggregateRootId, command.Title));
            Console.WriteLine("Handled CreateNoteCommand.");
        }
    }
}
