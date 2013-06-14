using System;

namespace ENode.Eventing
{
    /// <summary>Represents a concurrent exception when persisting the domain event.
    /// </summary>
    [Serializable]
    public class ConcurrentException : Exception
    {
        public ConcurrentException() : base() { }
        public ConcurrentException(string message) : base(message) { }
        public ConcurrentException(string message, Exception innerException) : base(message, innerException) { }
        public ConcurrentException(string message, params object[] args) : base(string.Format(message, args)) { }
    }
}
