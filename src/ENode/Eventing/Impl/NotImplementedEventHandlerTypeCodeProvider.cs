using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventHandlerTypeCodeProvider : DefaultTypeCodeProvider<IEventHandler>, ITypeCodeProvider<IEventHandler>
    {
    }
}
