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
        /// <summary>The code of exception type if the process has any exception.
        /// </summary>
        public int ExceptionCode { get; private set; }
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
        public ProcessResult(string processId, int exceptionCode, string errorMessage)
        {
            Status = ProcessStatus.Failed;
            ProcessId = processId;
            ExceptionCode = exceptionCode;
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
