using System;

namespace ENode.Domain
{
    /// <summary>Represents an aggregate root is a process.
    /// </summary>
    public interface IProcess
    {
        /// <summary>Represents whether the process is completed.
        /// </summary>
        bool IsCompleted { get; }
        /// <summary>Represents the result of the process.
        /// </summary>
        ProcessResult Result { get; }
    }
}
