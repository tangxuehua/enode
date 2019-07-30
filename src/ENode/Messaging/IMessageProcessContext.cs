namespace ENode.Infrastructure
{
    /// <summary>Represents the message processing context.
    /// </summary>
    public interface IMessageProcessContext
    {
        /// <summary>Notify the message has been processed.
        /// </summary>
        void NotifyMessageProcessed();
    }
}
