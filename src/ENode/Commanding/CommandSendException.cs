using System;

namespace ENode.Commanding
{
    /// <summary>Represents a command send exception.
    /// </summary>
    [Serializable]
    public class CommandSendException : Exception
    {
        /// <summary>Default constructor.
        /// </summary>
        public CommandSendException() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        public CommandSendException(string message) : base(message) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CommandSendException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="args"></param>
        public CommandSendException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }
    }
}
