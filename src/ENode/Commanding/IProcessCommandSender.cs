using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents a process command sender.
    /// </summary>
    public interface IProcessCommandSender
    {
        /// <summary>Send a process command synchronously.
        /// </summary>
        /// <param name="processCommand">The process command to send.</param>
        /// <param name="sourceEventId">The source domain event id.</param>
        void Send(IProcessCommand processCommand, string sourceEventId);
    }
}
