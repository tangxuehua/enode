using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a stream of domain event.
    /// <remarks>
    /// One stream may contains several domain events, but they must belong to a single aggregate.
    /// </remarks>
    /// </summary>
    [Serializable]
    public class EventStream : Message, IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootName"></param>
        /// <param name="version"></param>
        /// <param name="commandId"></param>
        /// <param name="timestamp"></param>
        /// <param name="events"></param>
        public EventStream(string aggregateRootId, string aggregateRootName, long version, Guid commandId, DateTime timestamp, IEnumerable<IEvent> events)
            : this(Guid.NewGuid(), aggregateRootId, aggregateRootName, version, commandId, timestamp, events)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootName"></param>
        /// <param name="version"></param>
        /// <param name="commandId"></param>
        /// <param name="timestamp"></param>
        /// <param name="events"></param>
        public EventStream(Guid id, string aggregateRootId, string aggregateRootName, long version, Guid commandId, DateTime timestamp, IEnumerable<IEvent> events)
            : base(id)
        {
            this.AggregateRootId = aggregateRootId;
            this.AggregateRootName = aggregateRootName;
            this.CommandId = commandId;
            this.Version = version;
            this.Timestamp = timestamp;
            this.Events = events ?? new List<IEvent>();
        }

        /// <summary>
        /// 
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string AggregateRootName { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CommandId { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public long Version { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IEvent> Events { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public bool HasEvent<TEvent>() where TEvent : class, IEvent
        {
            return Events.Any(x => x.GetType() == typeof(TEvent));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public TEvent FindEvent<TEvent>() where TEvent : class, IEvent
        {
            return Events.SingleOrDefault(x => x.GetType() == typeof(TEvent)) as TEvent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetEventInformation()
        {
            return string.Join("|", Events.Select(x => x.GetType().Name));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetStreamInformation()
        {
            var items = new List<object>
            {
                Id,
                AggregateRootName,
                AggregateRootId,
                Version,
                string.Join("-", Events.Select(x => x.GetType().Name)),
                CommandId,
                Timestamp
            };
            return string.Join("|", items);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetEventInformation();
        }
    }
}
