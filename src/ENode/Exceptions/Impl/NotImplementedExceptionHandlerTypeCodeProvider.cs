using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Exceptions.Impl
{
    public class NotImplementedExceptionHandlerTypeCodeProvider : DefaultTypeCodeProvider<IExceptionHandler>, ITypeCodeProvider<IExceptionHandler>
    {
    }
}
