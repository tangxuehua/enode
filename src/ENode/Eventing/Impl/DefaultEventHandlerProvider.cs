using System;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IMessageHandlerProvider<IEventHandler>.
    /// </summary>
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
