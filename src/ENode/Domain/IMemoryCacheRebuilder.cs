namespace ENode.Domain
{
    /// <summary>An interface to rebuild the whole domain by using event sourcing pattern.
    /// </summary>
    public interface IMemoryCacheRebuilder
    {
        /// <summary>Using event sourcing pattern to rebuild the domain by replaying all the domain events from the eventstore.
        /// </summary>
        void RebuildMemoryCache();
    }
}
