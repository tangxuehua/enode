using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a handled command which contains the command and the source info of the command.
    /// </summary>
    public class HandledCommand
    {
        /// <summary>The command.
        /// </summary>
        public ICommand Command { get; private set; }
        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The aggregate root type code.
        /// </summary>
        public int AggregateRootTypeCode { get; private set; }
        /// <summary>The identifier of the source.
        /// </summary>
        public string SourceId { get; private set; }
        /// <summary>The type of the source.
        /// </summary>
        public string SourceType { get; private set; }
        /// <summary>The result application message after the command is handled.
        /// </summary>
        public IApplicationMessage Message { get; private set; }

        /// <summary>Default constructor.
        /// </summary>
        public HandledCommand() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sourceId"></param>
        /// <param name="sourceType"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootTypeCode"></param>
        /// <param name="message"></param>
        public HandledCommand(ICommand command, string sourceId = null, string sourceType = null, string aggregateRootId = null, int aggregateRootTypeCode = 0, IApplicationMessage message = null)
        {
            Command = command;
            SourceId = sourceId;
            SourceType = sourceType;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
            Message = message;
        }

        /// <summary>Overrides to return the handled command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[CommandType={0},CommandId={1},SourceId={2},SourceType={3},AggregateRootId={4},AggregateRootTypeCode={5},Message={6}]",
                Command.GetType().Name,
                Command.Id,
                SourceId,
                SourceType,
                AggregateRootId,
                AggregateRootTypeCode,
                Message == null ? null : string.Format("[id:{0},type:{1}]", Message.Id, Message.GetType().Name));
        }
    }
}
