namespace ENode.Infrastructure
{
    /// <summary>Represents a context environment for processing data.
    /// </summary>
    public interface IProcessContext<T> where T : class
    {
        /// <summary>Notify the given data has been processed.
        /// </summary>
        /// <param name="data">The processed data.</param>
        void OnProcessed(T data);
    }
}
