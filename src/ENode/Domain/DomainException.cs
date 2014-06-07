using System;

namespace ENode.Domain
{
    public class DomainException : Exception
    {
        /// <summary>Default constructor.
        /// </summary>
        public DomainException() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        public DomainException(string message) : base(message) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public DomainException(string message, params object[] args) : base(string.Format(message, args)) { }
    }
}
