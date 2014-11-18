using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventHandlerProvider : BaseHandlerProvider<IEventHandler>, IHandlerProvider<IEventHandler>
    {
        protected override Type GetHandlerGenericInterfaceType()
        {
            return typeof(IEventHandler<>);
        }
        protected override Type GetHandlerProxyType()
        {
            return typeof(EventHandlerWrapper<>);
        }
    }
}
