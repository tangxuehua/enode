using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionHandlerProvider : BaseHandlerProvider<IExceptionHandler>, IHandlerProvider<IExceptionHandler>
    {
        protected override Type GetHandlerGenericInterfaceType()
        {
            return typeof(IExceptionHandler<>);
        }
        protected override Type GetHandlerProxyType()
        {
            return typeof(ExceptionHandlerWrapper<>);
        }
    }
}
