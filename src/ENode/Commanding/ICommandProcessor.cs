using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents a processor to process command.
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>Process the given command.
        /// </summary>
        /// <param name="processingCommand"></param>
        Task ProcessAsync(ProcessingCommand processingCommand);
        /// <summary>Start the processor.
        /// </summary>
        void Start();
        /// <summary>Stop the processor.
        /// </summary>
        void Stop();
    }
}
