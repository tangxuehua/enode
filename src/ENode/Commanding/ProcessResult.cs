using System;
using System.Collections.Generic;

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
        /// <summary>Represents the unique identifier of the aggregate which result the process complete.
        /// </summary>
        public string CompleteProcessAggregateRootId { get; private set; }
        /// <summary>Represents the error code if the process is not success.
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>Represents the exception type name if the process has any exception.
        /// </summary>
        public string ExceptionTypeName { get; private set; }
        /// <summary>Represents the error message if the process execution is failed.
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>Represents the extension information of the process result.
        /// </summary>
        public IDictionary<string, string> Items { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public ProcessResult(string processId, string completeProcessAggregateRootId, ProcessStatus status, int errorCode, string exceptionTypeName, string errorMessage, IDictionary<string, string> items)
        {
            ProcessId = processId;
            CompleteProcessAggregateRootId = completeProcessAggregateRootId;
            Status = status;
            ErrorCode = errorCode;
            ExceptionTypeName = exceptionTypeName;
            ErrorMessage = errorMessage;
            Items = items ?? new Dictionary<string, string>();
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
