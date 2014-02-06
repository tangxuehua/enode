using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>In-memory based ICommandIndexStore implementation. It is only used for unit test.
    /// </summary>
    public class InMemoryCommandIndexStore : ICommandIndexStore
    {
        public void Append(Guid commandId, long commitSequence)
        {
        }
        public IEnumerable<KeyValuePair<Guid, long>> Query(long startIndex, int size)
        {
            return new List<KeyValuePair<Guid, long>>();
        }
    }
}
