using System.Collections.Generic;

namespace ENode.Eventing.Impl
{
    public interface ICommitLog
    {
        /// <summary>Append the given event stream to the log, and returns the commit sequence.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        long Append(EventByteStream stream);
        /// <summary>Get the event stream by the commit sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        EventByteStream Get(long sequence);
        /// <summary>Query event commit records by start sequence and count.
        /// </summary>
        /// <param name="startSequence"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<CommitRecord> Query(long startSequence, int count);
    }
}
