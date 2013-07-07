namespace ENode.Eventing
{
    /// <summary>Represents a event persistence synchronizer.
    /// <remarks>
    ///  Code can be executed before and after the event stream persistence completion.
    /// </remarks>
    /// </summary>
    public interface IEventPersistenceSynchronizer
    {
        /// <summary>Indicates whether the synchronizer want to synchronize the given event stream.
        /// </summary>
        /// <returns></returns>
        bool IsSynchronizeTo(EventStream eventStream);
        /// <summary>Executed before persisting the event stream.
        /// </summary>
        void OnBeforePersisting(EventStream eventStream);
        /// <summary>Executed after the event stream was persisted.
        /// </summary>
        void OnAfterPersisted(EventStream eventStream);
    }
}
