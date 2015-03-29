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
        /// <summary>The result application message after the command is handled.
        /// </summary>
        public IApplicationMessage Message { get; private set; }

        /// <summary>Default constructor.
        /// </summary>
        public HandledCommand() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootTypeCode"></param>
        /// <param name="message"></param>
        public HandledCommand(ICommand command, string aggregateRootId = null, int aggregateRootTypeCode = 0, IApplicationMessage message = null)
        {
            Command = command;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
            Message = message;
        }

        /// <summary>Overrides to return the handled command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[CommandType={0},CommandId={1},AggregateRootId={2},AggregateRootTypeCode={3},Message={4}]",
                Command.GetType().Name,
                Command.Id,
                AggregateRootId,
                AggregateRootTypeCode,
                Message == null ? null : string.Format("[id:{0},type:{1}]", Message.Id, Message.GetType().Name));
        }
    }
}
