using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventing
{
    [Serializable]
    public class EventStream : IEventStream
    {
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

        public string CommandId { get; private set; }
        public string ProcessId { get; private set; }
        public IEnumerable<IEvent> Events { get; private set; }
        public IDictionary<string, string> Items { get; private set; }

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
