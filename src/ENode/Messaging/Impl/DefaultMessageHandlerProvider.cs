using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Messaging.Impl
{
    public class DefaultMessageHandlerProvider : BaseHandlerProvider<IMessageHandler>, IHandlerProvider<IMessageHandler>
    {
        protected override Type GetHandlerGenericInterfaceType()
        {
            return typeof(IMessageHandler<>);
        }
        protected override Type GetHandlerProxyType()
        {
            return typeof(MessageHandlerWrapper<>);
        }
    }
}
