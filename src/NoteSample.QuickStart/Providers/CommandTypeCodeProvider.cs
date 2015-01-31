using ECommon.Components;
using ENode.Commanding;
using ENode.Infrastructure.Impl;
using NoteSample.Commands;

namespace NoteSample.QuickStart.Providers
{
    [Component]
    public class CommandTypeCodeProvider : DefaultTypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateNoteCommand>(100);
            RegisterType<ChangeNoteTitleCommand>(101);
        }
    }
}
