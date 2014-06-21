using System;

namespace ENode.Commanding
{
    /// <summary>Represents a process result.
    /// </summary>
    [Serializable]
    public class ProcessResult
    {
        /// <summary>Represents the result status of the process.
        /// </summary>
        public ProcessStatus Status { get; private set; }
        /// <summary>Represents the unique identifier of the process.
        /// </summary>
        public string ProcessId { get; private set; }
        /// <summary>Represents the error code if the process is not success.
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>Represents the exception type name if the process has any exception.
        /// </summary>
        public string ExceptionTypeName { get; private set; }
        /// <summary>Represents the error message if the process execution is failed.
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
        public ProcessResult(string processId, int errorCode)
        {
            Status = ProcessStatus.Failed;
            ProcessId = processId;
            ErrorCode = errorCode;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public ProcessResult(string processId, string exceptionTypeName, string errorMessage)
        {
            Status = ProcessStatus.Failed;
            ProcessId = processId;
            ExceptionTypeName = exceptionTypeName;
            ErrorMessage = errorMessage;
        }
    }
    /// <summary>Represents a process result status enum.
    /// </summary>
    public enum ProcessStatus
    {
        Success = 1,
        Failed,
    }
}
