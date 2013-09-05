using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a provider to provide the event synchronizer information.
    /// </summary>
    public interface IEventSynchronizerProvider
    {
        /// <summary>Get all the event synchronizers for the given event type.
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        IEnumerable<IEventSynchronizer> GetSynchronizers(Type eventType);
    }
}
