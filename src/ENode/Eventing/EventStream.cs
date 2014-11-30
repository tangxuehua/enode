using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventing
{
    [Serializable]
    public class EventStream
    {
        public EventStream(string commandId, IEnumerable<IEvent> events, IDictionary<string, string> items)
        {
            if (events == null || events.Count() == 0)
            {
                throw new ArgumentException("Events cannot be null or empty.");
            }
            CommandId = commandId;
            Events = events;
            Items = items ?? new Dictionary<string, string>();
        }

        public string CommandId { get; private set; }
        public IEnumerable<IEvent> Events { get; private set; }
        public IDictionary<string, string> Items { get; private set; }

        public override string ToString()
        {
            var format = "[CommandId={0},Events={1},Items={2}]";
            return string.Format(format,
                CommandId,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
