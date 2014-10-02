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
        /// <param name="sourceEventId">The source event id.</param>
        /// <param name="sourceExceptionId">The source exception id.</param>
        void SendProcessCommand(ICommand processCommand, string sourceEventId, string sourceExceptionId);
    }
}
