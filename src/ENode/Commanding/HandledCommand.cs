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
        /// <param name="sourceEventId"></param>
        /// <param name="sourceExceptionId"></param>
        /// <param name="evnts"></param>
        public HandledCommand(ICommand command, string sourceEventId, string sourceExceptionId, IEnumerable<IEvent> evnts)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            Command = command;
            SourceEventId = sourceEventId;
            SourceExceptionId = sourceExceptionId;
            Events = evnts ?? new List<IEvent>();

            if (command is IStartProcessCommand)
            {
                ProcessId = ((IStartProcessCommand)command).ProcessId;
                if (string.IsNullOrEmpty(ProcessId))
                {
                    throw new CommandProcessIdMissingException(command);
                }
            }
            else if (command.Items.ContainsKey("ProcessId"))
            {
                ProcessId = command.Items["ProcessId"];
                if (string.IsNullOrEmpty(ProcessId))
                {
                    throw new CommandProcessIdMissingException(command);
                }
            }
        }

        /// <summary>The command object.
        /// </summary>
        public ICommand Command { get; private set; }
        /// <summary>The source event id.
        /// </summary>
        public string SourceEventId { get; private set; }
        /// <summary>The source exception id.
        /// </summary>
        public string SourceExceptionId { get; private set; }
        /// <summary>The process id.
        /// </summary>
        public string ProcessId { get; private set; }
        /// <summary>The events.
        /// </summary>
        public IEnumerable<IEvent> Events { get; private set; }

        /// <summary>Overrides to return the handled command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommandType={0},CommandId={1},SourceEventId={2},SourceExceptionId={3},ProcessId={4},Events={5},Items={6}]";
            return string.Format(format,
                Command.GetType().Name,
                Command.Id,
                SourceEventId,
                SourceExceptionId,
                ProcessId,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Command.Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
