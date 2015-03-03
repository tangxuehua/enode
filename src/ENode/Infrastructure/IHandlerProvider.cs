using System;
using System.Collections.Generic;

namespace ENode.Infrastructure
{
    /// <summary>Represents a provider to provide the message handlers.
    /// </summary>
    public interface IHandlerProvider
    {
        /// <summary>Get all the handlers for the given message type.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        IEnumerable<IProxyHandler> GetHandlers(Type messageType);
    }
}
