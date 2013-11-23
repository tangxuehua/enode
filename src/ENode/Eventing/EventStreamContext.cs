using System;
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
        public ConcurrentException ConcurrentException { get; private set; }

        public void SetConcurrentException(ConcurrentException concurrentException)
        {
            if (concurrentException == null)
            {
                throw new ArgumentNullException("concurrentException");
            }
            ConcurrentException = concurrentException;
        }
    }
}
