using ECommon.Components;
using ENode.Commanding;
using ENode.Infrastructure.Impl;
using NoteSample.Commands;

namespace DistributeSample.CommandProcessor.Providers
{
    [Component]
    public class CommandTypeCodeProvider : DefaultTypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
        }
    }
}
