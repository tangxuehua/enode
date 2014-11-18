namespace ENode.Infrastructure
{
    /// <summary>Represents a data processor.
    /// </summary>
    public interface IProcessor<T> where T : class
    {
        /// <summary>Gets or sets the name of the processor.
        /// </summary>
        string Name { get; set; }
        /// <summary>Start the processor.
        /// </summary>
        void Start();
        /// <summary>Process the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        void Process(T data, IProcessContext<T> context);
    }
}
