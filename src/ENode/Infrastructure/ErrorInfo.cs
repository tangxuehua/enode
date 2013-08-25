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

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="errorMessage"></param>
        public ErrorInfo(string errorMessage) : this(errorMessage, null) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="exception"></param>
        /// <exception cref="Exception"></exception>
        public ErrorInfo(string errorMessage, Exception exception)
        {
            if (string.IsNullOrEmpty(errorMessage) && exception == null)
            {
                throw new Exception("Invalid error info.");
            }
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        /// <summary>Returns the error message.
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            return !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage : Exception.Message;
        }
    }
}
