using System;

namespace ENode.Infrastructure
{
    /// <summary>A simple object which contains some error information.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>The error message.
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>The exception object.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
