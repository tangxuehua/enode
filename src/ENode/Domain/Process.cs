using System;

namespace ENode.Domain
{
    /// <summary>Abstract process base class with strong type process id.
    /// </summary>
    [Serializable]
    public abstract class Process<TProcessId> : AggregateRoot<TProcessId>, IProcess
    {
        protected Process() : base() { }
        public Process(TProcessId id) : base(id) { }

        /// <summary>Represents whether the process is completed.
        /// </summary>
        public bool IsCompleted { get; protected set; }
        /// <summary>Represents the result of the process.
        /// </summary>
        public ProcessResult Result { get; protected set; }
    }
}
