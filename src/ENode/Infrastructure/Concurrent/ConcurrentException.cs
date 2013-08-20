using System;

namespace ENode.Infrastructure.Concurrent
{
    /// <summary>Represents a concurrent exception.
    /// </summary>
    [Serializable]
    public class ConcurrentException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentException() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ConcurrentException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ConcurrentException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public ConcurrentException(string message, params object[] args) : base(string.Format(message, args)) { }
    }
}
