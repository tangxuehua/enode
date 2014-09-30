using System;
using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    /// <summary>The default implementation of IMessageHandlerProvider<IExceptionHandler>.
    /// </summary>
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
