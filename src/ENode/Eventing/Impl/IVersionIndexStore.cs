using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl
{
    public interface IVersionIndexStore
    {
        /// <summary>Append a version index.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="commitSequence"></param>
        void Append(string key, long commitSequence);
        /// <summary>Query version index entries by start index and size.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, long>> Query(long startIndex, int size);
    }
}
