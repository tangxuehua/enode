namespace ENode.Distribute.EventStore
{
    public interface IEventStoreServer
    {
        /// <summary>Start the event store server.
        /// </summary>
        void Start();
        /// <summary>Shutdown the event store server.
        /// </summary>
        void Shutdown();
    }
}
