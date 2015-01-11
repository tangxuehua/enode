using System.Collections.Generic;

namespace ENode.Infrastructure
{
    public interface IDispatcher<TMessage> where TMessage : class, IDispatchableMessage
    {
        bool DispatchMessage(TMessage message);
        bool DispatchMessages(IEnumerable<TMessage> messages);
    }
}
