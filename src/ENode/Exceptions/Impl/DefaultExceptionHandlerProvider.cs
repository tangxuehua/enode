using System;
using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionHandlerProvider : BaseHandlerProvider<IExceptionHandler>, IMessageHandlerProvider<IExceptionHandler>, IAssemblyInitializer
    {
        protected override Type GetMessageHandlerGenericInterfaceType()
        {
            return typeof(IExceptionHandler<>);
        }
        protected override Type GetMessageHandlerWrapperType()
        {
            return typeof(ExceptionHandlerWrapper<>);
        }
    }
}
