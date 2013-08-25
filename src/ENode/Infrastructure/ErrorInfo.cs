using System;

namespace ENode.Infrastructure
{
    /// <summary>A simple object which contains some error information.
    /// </summary>
    [Serializable]
    public class ErrorInfo
    {
        /// <summary>The error message.
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>The exception object.
        /// </summary>
        public Exception Exception { get; private set; }

        public ErrorInfo(string errorMessage) : this(errorMessage, null) { }
        public ErrorInfo(string errorMessage, Exception exception)
        {
            if (string.IsNullOrEmpty(errorMessage) && exception == null)
            {
                throw new Exception("Invalid error info.");
            }
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public string GetErrorMessage()
        {
            return !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage : Exception.Message;
        }
    }
}
