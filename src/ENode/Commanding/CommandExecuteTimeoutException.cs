using System;

namespace ENode.Commanding
{
    public class CommandExecuteTimeoutException : Exception
    {
        /// <summary>Default constructor.
        /// </summary>
        public CommandExecuteTimeoutException() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        public CommandExecuteTimeoutException(string message) : base(message) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CommandExecuteTimeoutException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="args"></param>
        public CommandExecuteTimeoutException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }
    }
}
