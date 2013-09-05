namespace ENode.Eventing
{
    /// <summary>Represents a event persistence synchronizer.
    /// <remarks>
    ///  Code can be executed before and after the event persistence.
    /// </remarks>
    /// </summary>
    public interface IEventSynchronizer
    {
        /// <summary>Executed before persisting the event.
        /// </summary>
        void OnBeforePersisting(IEvent evnt);
        /// <summary>Executed after the event was persisted.
        /// </summary>
        void OnAfterPersisted(IEvent evnt);
        /// <summary>Represents the inner generic IEventSynchronizer.
        /// </summary>
        object GetInnerSynchronizer();
    }

    /// <summary>Represents a event persistence synchronizer.
    /// <remarks>
    ///  Code can be executed before and after the event persistence.
    /// </remarks>
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventSynchronizer<in TEvent> where TEvent : class, IEvent
    {
        /// <summary>Executed before persisting the event.
        /// </summary>
        void OnBeforePersisting(TEvent evnt);
        /// <summary>Executed after the event was persisted.
        /// </summary>
        void OnAfterPersisted(TEvent evnt);
    }
}
