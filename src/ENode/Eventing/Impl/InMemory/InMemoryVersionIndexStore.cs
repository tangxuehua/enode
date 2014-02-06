using System.Collections.Generic;

namespace ENode.Eventing.Impl.InMemory
{
    public class InMemoryVersionIndexStore : IVersionIndexStore
    {
        public void Append(string key, long commitSequence)
        {
        }
        public IEnumerable<KeyValuePair<string, long>> Query(long startIndex, int size)
        {
            return new List<KeyValuePair<string, long>>();
        }
    }
}
