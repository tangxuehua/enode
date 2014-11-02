using DistributeSample.Commands;
using ECommon.Components;
using ENode.Commanding;
using ENode.Infrastructure;

namespace DistributeSample.CommandProducer.Providers
{
    [Component]
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
        }
    }
}
