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
        IEnumerable<MessageHandlerData<IMessageHandlerProxy1>> GetHandlers(Type messageType);
    }
    /// <summary>Represents a provider to provide the handlers which handle two messages.
    /// </summary>
    public interface ITwoMessageHandlerProvider
    {
        /// <summary>Get all the handlers for the given message types.
        /// </summary>
        /// <param name="messageTypes"></param>
        /// <returns></returns>
        IEnumerable<MessageHandlerData<IMessageHandlerProxy2>> GetHandlers(IEnumerable<Type> messageTypes);
    }
    /// <summary>Represents a provider to provide the handlers which handle three messages.
    /// </summary>
    public interface IThreeMessageHandlerProvider
    {
        /// <summary>Get all the handlers for the given message types.
        /// </summary>
        /// <param name="messageTypes"></param>
        /// <returns></returns>
        IEnumerable<MessageHandlerData<IMessageHandlerProxy3>> GetHandlers(IEnumerable<Type> messageTypes);
    }
}
