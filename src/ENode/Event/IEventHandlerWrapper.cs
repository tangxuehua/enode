using System;

namespace ENode.Eventing
{
    public interface IEventHandlerWrapper
    {
        object GetInnerEventHandler();
    }
}
