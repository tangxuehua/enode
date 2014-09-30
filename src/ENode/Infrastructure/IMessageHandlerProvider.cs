using System;
using System.Collections.Generic;

namespace ENode.Infrastructure
{
    /// <summary>Represents a provider to provide the message handler information.
    /// </summary>
    public interface IMessageHandlerProvider<TMessageHandlerInterface> where TMessageHandlerInterface : class
    {
        /// <summary>Get all the message handlers for the given message type.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        IEnumerable<TMessageHandlerInterface> GetMessageHandlers(Type messageType);
        /// <summary>Check whether a given type is a message handler type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsMessageHandler(Type type);
    }
}
