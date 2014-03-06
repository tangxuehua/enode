using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a duplicate aggregate creation exception.
    /// </summary>
    [Serializable]
    public class DuplicateAggregateException : ENodeException
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootTypeCode"></param>
        /// <param name="aggregateRootId"></param>
        public DuplicateAggregateException(int aggregateRootTypeCode, string aggregateRootId)
            : base(string.Format("Duplicate aggregate[typeCode={0},id={1}] creation.", aggregateRootTypeCode, aggregateRootId)) { }
    }
}
