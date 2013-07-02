using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a provider to provide the event persistence synchronizer information.
    /// </summary>
    public interface IEventPersistenceSynchronizerProvider
    {
        /// <summary>Get all the event persistence synchronizers for the given event stream.
        /// </summary>
        /// <param name="eventStream"></param>
        /// <returns></returns>
        IEnumerable<IEventPersistenceSynchronizer> GetSynchronizers(EventStream eventStream);
    }
}
