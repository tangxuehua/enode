namespace ENode.Commanding
{
    /// <summary>Represents a processor to process command.
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>Process the given command.
        /// </summary>
        /// <param name="processingCommand"></param>
        void Process(ProcessingCommand processingCommand);
    }
}
