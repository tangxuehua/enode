using ENode.Commanding;
using ENode.Infrastructure;
using NoteSample.Commands;

namespace NoteSample.Providers
{
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider<ICommand>, ITypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
            RegisterType<ChangeNoteTitleCommand>(101);
        }
    }
}
