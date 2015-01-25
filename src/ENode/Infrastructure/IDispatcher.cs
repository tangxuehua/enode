using System.Collections.Generic;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message dispatcher.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IDispatcher<TMessage> where TMessage : class, IDispatchableMessage
    {
        /// <summary>Dispatch the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DispatchMessage(TMessage message);
        /// <summary>Dispatch the given messages.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        bool DispatchMessages(IEnumerable<TMessage> messages);
    }
}
