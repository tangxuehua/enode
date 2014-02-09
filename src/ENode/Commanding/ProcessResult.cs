using System;

namespace ENode.Commanding
{
    /// <summary>Represents a process result.
    /// </summary>
    [Serializable]
    public class ProcessResult
    {
        /// <summary>The status of the process.
        /// </summary>
        public ProcessStatus Status { get; private set; }
        /// <summary>The uniqueId of the process.
        /// </summary>
        public string ProcessId { get; private set; }
        /// <summary>If the process is not success, then this property contains the error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public ProcessResult(string processId)
        {
            Status = ProcessStatus.Success;
            ProcessId = processId;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public ProcessResult(string processId, Exception exception) : this(processId, exception.Message) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public ProcessResult(string processId, string errorMessage)
        {
            Status = ProcessStatus.Failed;
            ProcessId = processId;
            ErrorMessage = errorMessage;
        }
    }
    /// <summary>Represents a process processing status.
    /// </summary>
    public enum ProcessStatus
    {
        Success = 1,
        Failed,
    }
}
