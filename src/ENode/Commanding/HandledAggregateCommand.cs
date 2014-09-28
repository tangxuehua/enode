using System;
using System.Linq;

namespace ENode.Commanding
{
    /// <summary>Represents a handled aggregate command which contains the command and the event stream key information.
    /// </summary>
    public class HandledAggregateCommand
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sourceEventId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootTypeCode"></param>
        public HandledAggregateCommand(ICommand command, string sourceEventId, string aggregateRootId, int aggregateRootTypeCode)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            Command = command;
            SourceEventId = sourceEventId;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;

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
        /// <summary>The source domain event id.
        /// </summary>
        public string SourceEventId { get; private set; }
        /// <summary>The aggregate root type code.
        /// </summary>
        public int AggregateRootTypeCode { get; private set; }
        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The process id.
        /// </summary>
        public string ProcessId { get; private set; }

        /// <summary>Overrides to return the handled aggregate command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommandType={0},CommandId={1},SourceEventId={2},AggregateRootTypeCode={3},AggregateRootId={4},ProcessId={5},Items={6}]";
            return string.Format(format,
                Command.GetType().Name,
                Command.Id,
                SourceEventId,
                AggregateRootTypeCode,
                AggregateRootId,
                ProcessId,
                string.Join("|", Command.Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
