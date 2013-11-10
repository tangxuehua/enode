namespace ENode.Eventing
{
    /// <summary>Represents a storage to store the event publish information of aggregate.
    /// </summary>
    public interface IEventPublishInfoStore
    {
        /// <summary>Insert the first published event version of aggregate.
        /// </summary>
        void InsertFirstPublishedVersion(object aggregateRootId);
        /// <summary>Update the published event version of aggregate.
        /// </summary>
        void UpdatePublishedVersion(object aggregateRootId, long version);
        /// <summary>Get the current event published version for the specified aggregate.
        /// </summary>
        long GetEventPublishedVersion(object aggregateRootId);
    }
}
