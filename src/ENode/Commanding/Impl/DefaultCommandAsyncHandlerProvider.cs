using System;
using ENode.Infrastructure.Impl;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandAsyncHandlerProvider : AbstractHandlerProvider<ICommandAsyncHandlerProxy>, ICommandAsyncHandlerProvider
    {
        protected override Type GetGenericHandlerType()
        {
            return typeof(ICommandAsyncHandler<>);
        }
        protected override Type GetHandlerProxyImplementationType()
        {
            return typeof(CommandAsyncHandlerProxy<>);
        }
    }
}
