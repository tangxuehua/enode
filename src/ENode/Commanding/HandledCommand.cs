using System;
using System.Linq;

namespace ENode.Commanding
{
    /// <summary>Represents a handled command which contains the command and the event stream key information.
    /// </summary>
    public class HandledCommand
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootTypeCode"></param>
        public HandledCommand(ICommand command, string aggregateRootId, int aggregateRootTypeCode)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            Command = command;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
        }

        /// <summary>The command object.
        /// </summary>
        public ICommand Command { get; private set; }
        /// <summary>The aggregate root type code.
        /// </summary>
        public int AggregateRootTypeCode { get; private set; }
        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }

        /// <summary>Overrides to return the handled command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommandType={0},CommandId={1},AggregateRootTypeCode={2},AggregateRootId={3},ProcessId={4},Items={5}]";
            return string.Format(format,
                Command.GetType().Name,
                Command.Id,
                AggregateRootTypeCode,
                AggregateRootId,
                Command is IProcessCommand ? ((IProcessCommand)Command).ProcessId : null,
                string.Join("|", Command.Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
