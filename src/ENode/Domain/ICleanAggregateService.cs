namespace ENode.Domain
{
    /// <summary>A service to clean aggregates from memory by strategy.
    /// </summary>
    public interface ICleanAggregateService
    {
        /// <summary>Clean in-memory aggregates, remove aggregates by strategy.
        /// </summary>
        void Clean();
    }
}
