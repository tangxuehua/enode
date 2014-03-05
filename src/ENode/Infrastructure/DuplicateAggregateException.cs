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
        /// <param name="aggregateRootName"></param>
        /// <param name="aggregateRootId"></param>
        public DuplicateAggregateException(string aggregateRootName, string aggregateRootId)
            : base(string.Format("Duplicate aggregate[name={0},id={1}] creation.", aggregateRootName, aggregateRootId)) { }
    }
}
