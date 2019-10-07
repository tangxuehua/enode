using System.Collections.Generic;
using ENode.Infrastructure;

namespace ENode.Messaging
{
    public class MessageHandlerData<T> where T : IObjectProxy
    {
        public IEnumerable<T> AllHandlers = new List<T>();
        public IEnumerable<T> ListHandlers = new List<T>();
        public IEnumerable<T> QueuedHandlers = new List<T>();
    }
}
