using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents an event.
    /// </summary>
    public interface IEventSerializer
    {
        /// <summary>Serialize the given events to dictionary.
        /// </summary>
        /// <param name="evnts"></param>
        /// <returns></returns>
        IDictionary<string, string> Serialize(IEnumerable<IDomainEvent> evnts);
        /// <summary>Deserialize the given data to events.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        IEnumerable<TEvent> Deserialize<TEvent>(IDictionary<string, string> data) where TEvent : class, IDomainEvent;
    }
}
