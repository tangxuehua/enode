using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a provider to provide the event handler information.
    /// </summary>
    public interface IEventHandlerProvider
    {
        /// <summary>Get all the event handlers for the given event type.
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        IEnumerable<IEventHandler> GetEventHandlers(Type eventType);
    }
}
