using ECommon.Components;
using ENode.Commanding;
using ENode.Infrastructure;
using NoteSample.Commands;

namespace NoteSample.Providers
{
    [Component]
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
            RegisterType<ChangeNoteTitleCommand>(101);
        }
    }
}
