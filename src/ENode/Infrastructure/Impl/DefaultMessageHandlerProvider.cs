using System;

namespace ENode.Infrastructure.Impl
{
    public class DefaultMessageHandlerProvider : AbstractHandlerProvider<IMessageHandlerProxy>, IMessageHandlerProvider
    {
        protected override Type GetGenericHandlerType()
        {
            return typeof(IMessageHandler<>);
        }
        protected override Type GetHandlerProxyImplementationType()
        {
            return typeof(MessageHandlerProxy<>);
        }
    }
}
