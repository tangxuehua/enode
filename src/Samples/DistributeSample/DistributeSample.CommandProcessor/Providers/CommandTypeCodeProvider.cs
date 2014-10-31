using DistributeSample.Commands;
using ECommon.Components;
using ENode.Commanding;
using ENode.Infrastructure;

namespace DistributeSample.CommandProcessor.Providers
{
    [Component(LifeStyle.Singleton)]
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
        }
    }
}
