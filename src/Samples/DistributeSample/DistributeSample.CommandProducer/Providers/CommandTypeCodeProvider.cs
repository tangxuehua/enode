using DistributeSample.Commands;
using ENode.Commanding;
using ENode.Infrastructure;

namespace DistributeSample.CommandProducer.Providers
{
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider<ICommand>, ITypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
        }
    }
}
