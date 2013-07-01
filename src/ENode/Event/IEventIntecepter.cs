namespace ENode.Eventing
{
    /// <summary>Represents an inteceptor which will be called before or after the domain event was persisted.
    /// </summary>
    public interface IEventInteceptor
    {
        /// <summary>Called before the event stream was persisted.
        /// </summary>
        void OnBeforePersisted(EventStream eventStream);
        /// <summary>Called after the event stream was persisted.
        /// </summary>
        void OnAfterPersisted(EventStream eventStream);
    }
}
