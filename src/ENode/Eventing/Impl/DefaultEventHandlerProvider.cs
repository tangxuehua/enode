using System;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class DefaultEventHandlerProvider : BaseHandlerProvider<IEventHandler>, IMessageHandlerProvider<IEventHandler>, IAssemblyInitializer
    {
        protected override Type GetMessageHandlerGenericInterfaceType()
        {
            return typeof(IEventHandler<>);
        }
        protected override Type GetMessageHandlerWrapperType()
        {
            return typeof(EventHandlerWrapper<>);
        }
    }
}
