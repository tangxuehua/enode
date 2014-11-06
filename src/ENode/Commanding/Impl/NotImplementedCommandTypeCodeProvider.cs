using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Commanding.Impl
{
    public class NotImplementedCommandTypeCodeProvider : DefaultTypeCodeProvider<ICommand>, ITypeCodeProvider<ICommand>
    {
    }
}
