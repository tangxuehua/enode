using System;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract process command which associated with a specific business process.
    /// </summary>
    [Serializable]
    public abstract class ProcessCommand<TAggregateRootId> : Command<TAggregateRootId>, IProcessCommand
    {
        public abstract string ProcessId { get; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        protected ProcessCommand(TAggregateRootId aggregateRootId) : base(aggregateRootId) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="retryCount"></param>
        protected ProcessCommand(TAggregateRootId aggregateRootId, int retryCount) : base(aggregateRootId, retryCount)
        {
        }

        /// <summary>Returns the command type name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
