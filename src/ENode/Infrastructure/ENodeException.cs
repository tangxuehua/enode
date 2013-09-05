using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a concurrent exception.
    /// </summary>
    [Serializable]
    public class ENodeException : Exception
    {
        /// <summary>Default constructor.
        /// </summary>
        public ENodeException() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        public ENodeException(string message) : base(message) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ENodeException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public ENodeException(string message, params object[] args) : base(string.Format(message, args)) { }
    }
}
