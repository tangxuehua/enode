using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a duplicate aggregate exception.
    /// </summary>
    [Serializable]
    public class DuplicateAggregateException : Exception
    {
        /// <summary>Default constructor.
        /// </summary>
        public DuplicateAggregateException() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        public DuplicateAggregateException(string message) : base(message) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DuplicateAggregateException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public DuplicateAggregateException(string message, params object[] args) : base(string.Format(message, args)) { }
    }
}
