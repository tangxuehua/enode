namespace ENode.Messaging
{
    /// <summary>Represents a processor to process the processed messages.
    /// </summary>
    public interface IProcessedMessageProcessor : IMessageProcessor<IProcessedMessageQueue, IMessage>
    {
    }
}
