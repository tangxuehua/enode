namespace ENode.Eventing
{
    /// <summary>Represents a storage to store the published version of aggregate.
    /// </summary>
    public interface IAggregatePublishVersionStore
    {
        /// <summary>Insert the first published version for the specified aggregate.
        /// </summary>
        void InsertFirstVersion(string eventProcessorName, string aggregateRootId);
        /// <summary>Update the published version for the specified aggregate.
        /// </summary>
        void UpdateVersion(string eventProcessorName, string aggregateRootId, int version);
        /// <summary>Get the current published version for the specified aggregate.
        /// </summary>
        int GetVersion(string eventProcessorName, string aggregateRootId);
    }
}
