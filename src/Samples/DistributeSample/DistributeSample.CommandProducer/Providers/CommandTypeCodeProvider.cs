using DistributeSample.Commands;
using ENode.Commanding;
using ENode.Infrastructure;

namespace DistributeSample.CommandProducer.Providers
{
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider, ICommandTypeCodeProvider
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
        }
    }
}
