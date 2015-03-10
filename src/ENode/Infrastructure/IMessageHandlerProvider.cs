using System;
using System.Collections.Generic;

namespace ENode.Infrastructure
{
    /// <summary>Represents a provider to provide the message handlers.
    /// </summary>
    public interface IMessageHandlerProvider
    {
        /// <summary>Get all the handlers for the given message type.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        IEnumerable<IMessageHandlerProxy> GetHandlers(Type messageType);
    }
}
