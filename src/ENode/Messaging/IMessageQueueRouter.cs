namespace ENode.Messaging
{
    /// <summary>Represents a local in-memory based message queue router.
    /// </summary>
    public interface IMessageQueueRouter<TQueue, TMessagePayload>
        where TQueue : class, IMessageQueue<TMessagePayload>
        where TMessagePayload : class, IPayload
    {
        /// <summary>Route a queue for the given message payload.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        TQueue Route(TMessagePayload payload);
    }
}
