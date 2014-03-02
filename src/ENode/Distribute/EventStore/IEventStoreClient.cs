namespace ENode.Distribute.EventStore
{
    public interface IEventStoreClient
    {
        /// <summary>Start the event store client.
        /// </summary>
        void Start();
        /// <summary>Shutdown the event store client.
        /// </summary>
        void Shutdown();
    }
}
