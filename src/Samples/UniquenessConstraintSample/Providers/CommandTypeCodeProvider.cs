using ENode.Commanding;
using ENode.Infrastructure;

namespace UniquenessConstraintSample.Providers
{
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider<ICommand>, ITypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateSectionCommand>(100);
            RegisterType<ChangeSectionNameCommand>(101);
        }
    }
}
