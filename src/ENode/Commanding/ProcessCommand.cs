using System;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract process command which associated with a specific business process.
    /// </summary>
    [Serializable]
    public abstract class ProcessCommand<TAggregateRootId> : Command<TAggregateRootId>, IProcessCommand
    {
        private string _processId;

        /// <summary>Represents the associated processId.
        /// </summary>
        string IProcessCommand.ProcessId
        {
            get { return _processId; }
        }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        protected ProcessCommand(TAggregateRootId aggregateRootId) : this(aggregateRootId, aggregateRootId) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="processId"></param>
        protected ProcessCommand(TAggregateRootId aggregateRootId, object processId)
            : this(aggregateRootId, processId, DefaultRetryCount)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="processId"></param>
        /// <param name="retryCount"></param>
        protected ProcessCommand(TAggregateRootId aggregateRootId, object processId, int retryCount)
            : base(aggregateRootId, retryCount)
        {
            if (processId == null)
            {
                throw new ArgumentNullException("processId");
            }
            _processId = processId.ToString();
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
