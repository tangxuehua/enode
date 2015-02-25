using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents an async operation result.
    /// </summary>
    public class AsyncOperationResult
    {
        public readonly static AsyncOperationResult Success = new AsyncOperationResult(AsyncOperationResultStatus.Success, null);

        /// <summary>Represents the publish result status.
        /// </summary>
        public AsyncOperationResultStatus Status { get; private set; }
        /// <summary>Represents the error message if publish message is failed.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncOperationResult(AsyncOperationResultStatus status, string errorMessage)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }
    }
    /// <summary>Represents an async operation result.
    /// </summary>
    public class AsyncOperationResult<T> : AsyncOperationResult
    {
        /// <summary>Represents the result data.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncOperationResult(AsyncOperationResultStatus status, T data)
            : this(status, null, data)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncOperationResult(AsyncOperationResultStatus status, string errorMessage)
            : this(status, errorMessage, default(T))
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        public AsyncOperationResult(AsyncOperationResultStatus status, string errorMessage, T data)
            : base(status, errorMessage)
        {
            Data = data;
        }
    }
    /// <summary>Represents an async operation result status enum.
    /// </summary>
    public enum AsyncOperationResultStatus
    {
        Success = 1,
        IOException = 2
    }
}
