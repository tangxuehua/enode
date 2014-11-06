using System.Linq;

namespace ENode.Commanding
{
    /// <summary>Represents a handled aggregate command which contains the command and the event stream key information.
    /// </summary>
    public class HandledAggregateCommand : HandledCommand
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sourceEventId"></param>
        /// <param name="sourceExceptionId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootTypeCode"></param>
        public HandledAggregateCommand(ICommand command, string sourceEventId, string sourceExceptionId, string aggregateRootId, int aggregateRootTypeCode)
            : base(command, sourceEventId, sourceExceptionId, null)
        {
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
        }

        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The aggregate root type code.
        /// </summary>
        public int AggregateRootTypeCode { get; private set; }

        /// <summary>Overrides to return the handled aggregate command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommandType={0},CommandId={1},SourceEventId={2},SourceExceptionId={3},AggregateRootTypeCode={4},AggregateRootId={5},Items={6}]";
            return string.Format(format,
                Command.GetType().Name,
                Command.Id,
                SourceEventId,
                SourceExceptionId,
                AggregateRootTypeCode,
                AggregateRootId,
                string.Join("|", Command.Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
