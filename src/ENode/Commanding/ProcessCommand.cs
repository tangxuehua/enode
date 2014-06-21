using System;
using ECommon.Utilities;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract process command which associated with a specific business process.
    /// </summary>
    [Serializable]
    public abstract class ProcessCommand<TAggregateRootId> : Command<TAggregateRootId>, IProcessCommand
    {
        /// <summary>Represents the process id.
        /// </summary>
        public string ProcessId { get; private set; }

        /// <summary>Default constructor.
        /// </summary>
        protected ProcessCommand() : this(ObjectId.GenerateNewStringId()) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processId"></param>
        protected ProcessCommand(object processId) : this(processId, default(TAggregateRootId)) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="aggregateRootId"></param>
        protected ProcessCommand(object processId, TAggregateRootId aggregateRootId) : this(processId, aggregateRootId, DefaultRetryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="retryCount"></param>
        protected ProcessCommand(object processId, TAggregateRootId aggregateRootId, int retryCount) : base(aggregateRootId, retryCount)
        {
            if (processId == null)
            {
                throw new ArgumentNullException("processId");
            }
            ProcessId = processId.ToString();
        }
    }
}
