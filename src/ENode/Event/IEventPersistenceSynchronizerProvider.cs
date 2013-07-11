using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a provider to provide the event persistence synchronizer information.
    /// </summary>
    public interface IEventPersistenceSynchronizerProvider
    {
        /// <summary>Get all the event persistence synchronizers for the given event type.
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        IEnumerable<IEventPersistenceSynchronizer> GetSynchronizers(Type eventType);
    }
}
