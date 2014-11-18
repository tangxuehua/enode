using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;

namespace ENode.Commanding
{
    /// <summary>Represents a handled command which contains the command and the event stream information.
    /// </summary>
    public class HandledCommand
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sourceId"></param>
        /// <param name="sourceType"></param>
        /// <param name="evnts"></param>
        public HandledCommand(ICommand command, string sourceId, string sourceType, IEnumerable<IEvent> evnts)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            Command = command;
            SourceId = sourceId;
            SourceType = sourceType;
            Events = evnts ?? new List<IEvent>();
        }

        /// <summary>The command object.
        /// </summary>
        public ICommand Command { get; private set; }
        /// <summary>The identifier of the source.
        /// </summary>
        public string SourceId { get; private set; }
        /// <summary>The type of the source.
        /// </summary>
        public string SourceType { get; private set; }
        /// <summary>The events.
        /// </summary>
        public IEnumerable<IEvent> Events { get; private set; }

        /// <summary>Overrides to return the handled command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommandType={0},CommandId={1},SourceId={2},SourceType={3},Events={4},Items={5}]";
            return string.Format(format,
                Command.GetType().Name,
                Command.Id,
                SourceId,
                SourceType,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Command.Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
