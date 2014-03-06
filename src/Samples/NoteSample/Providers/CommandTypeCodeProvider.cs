using ENode.Commanding;
using ENode.Infrastructure;
using NoteSample.Commands;

namespace NoteSample.Providers
{
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider, ICommandTypeCodeProvider
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
            RegisterType<ChangeNoteTitleCommand>(101);
        }
    }
}
