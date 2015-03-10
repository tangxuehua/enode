using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents an async task result.
    /// </summary>
    public class AsyncTaskResult
    {
        public readonly static AsyncTaskResult Success = new AsyncTaskResult(AsyncTaskStatus.Success, null);

        /// <summary>Represents the async task result status.
        /// </summary>
        public AsyncTaskStatus Status { get; private set; }
        /// <summary>Represents the error message if the async task is failed.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncTaskResult(AsyncTaskStatus status, string errorMessage)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }
    }
    /// <summary>Represents a generic async task result.
    /// </summary>
    public class AsyncTaskResult<T> : AsyncTaskResult
    {
        /// <summary>Represents the async task result data.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncTaskResult(AsyncTaskStatus status)
            : this(status, null, default(T))
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncTaskResult(AsyncTaskStatus status, T data)
            : this(status, null, data)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncTaskResult(AsyncTaskStatus status, string errorMessage)
            : this(status, errorMessage, default(T))
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncTaskResult(AsyncTaskStatus status, string errorMessage, T data)
            : base(status, errorMessage)
        {
            Data = data;
        }
    }
    /// <summary>Represents an async task result status enum.
    /// </summary>
    public enum AsyncTaskStatus
    {
        Success = 1,
        IOException = 2,
        Failed
    }
}
