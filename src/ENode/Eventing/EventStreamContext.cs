using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Concurrent;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>An internal class to contains the context information when processing an event stream.
    /// </summary>
    internal class EventStreamContext
    {
        public EventStream EventStream { get; set; }
        public IMessageQueue<EventStream> Queue { get; set; }
        public bool HasConcurrentException { get; private set; }
        public ErrorInfo ErrorInfo { get; private set; }

        public void SetConcurrentException(ErrorInfo errorInfo)
        {
            if (errorInfo == null)
            {
                throw new ArgumentNullException("errorInfo");
            }
            if (!(errorInfo.Exception is ConcurrentException))
            {
                throw new InvalidOperationException(string.Format("Unknown exception {0} cannot be set as concurrent exception.", errorInfo.Exception.GetType().Name));
            }
            HasConcurrentException = true;
            ErrorInfo = errorInfo;
        }
    }
}
