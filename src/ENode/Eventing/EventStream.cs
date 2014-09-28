using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventing
{
    /// <summary>Represents a stream of event, the stream may contains several events.
    /// </summary>
    [Serializable]
    public class EventStream
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="processId"></param>
        /// <param name="events"></param>
        /// <param name="items"></param>
        public EventStream(string commandId, string processId, IEnumerable<IEvent> events, IDictionary<string, string> items)
        {
            if (events == null || events.Count() == 0)
            {
                throw new ArgumentException("Events cannot be null or empty.");
            }
            CommandId = commandId;
            ProcessId = processId;
            Events = events;
            Items = items ?? new Dictionary<string, string>();
        }

        /// <summary>The commandId which generates this event stream.
        /// </summary>
        public string CommandId { get; private set; }
        /// <summary>The process id which the current event associated.
        /// </summary>
        public string ProcessId { get; private set; }
        /// <summary>The events of the event stream.
        /// </summary>
        public IEnumerable<IEvent> Events { get; private set; }
        /// <summary>Represents the extension information of the current event stream.
        /// </summary>
        public IDictionary<string, string> Items { get; private set; }

        /// <summary>Overrides to return the whole event stream information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommandId={0},ProcessId={1},Events={2},Items={3}]";
            return string.Format(format,
                CommandId,
                ProcessId,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
