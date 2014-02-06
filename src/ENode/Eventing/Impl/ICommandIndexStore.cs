using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl
{
    public interface ICommandIndexStore
    {
        /// <summary>Append a command index.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="commitSequence"></param>
        void Append(Guid commandId, long commitSequence);
        /// <summary>Query command index entries by start index and size.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<Guid, long>> Query(long startIndex, int size);
    }
}
