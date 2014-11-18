using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandHandlerProvider : BaseHandlerProvider<ICommandHandler>, IHandlerProvider<ICommandHandler>
    {
        protected override Type GetHandlerGenericInterfaceType()
        {
            return typeof(ICommandHandler<>);
        }
        protected override Type GetHandlerProxyType()
        {
            return typeof(CommandHandlerWrapper<>);
        }
    }
}
