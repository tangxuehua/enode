using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a handled command which contains the command and the source info of the command.
    /// </summary>
    public class HandledCommand
    {
        /// <summary>The command id.
        /// </summary>
        public string CommandId { get; private set; }
        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The result application message after the command is handled.
        /// </summary>
        public IApplicationMessage Message { get; private set; }

        /// <summary>Default constructor.
        /// </summary>
        public HandledCommand() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="message"></param>
        public HandledCommand(string commandId, string aggregateRootId = null, IApplicationMessage message = null)
        {
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            Message = message;
        }

        /// <summary>Overrides to return the handled command's useful information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[CommandId={0},AggregateRootId={1},Message={2}]",
                CommandId,
                AggregateRootId,
                Message == null ? null : string.Format("[id:{0},type:{1}]", Message.Id, Message.GetType().Name));
        }
    }
}
