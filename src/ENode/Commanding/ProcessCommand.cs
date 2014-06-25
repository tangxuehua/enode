using System;
using ECommon.Utilities;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract process command which associated with a specific business process.
    /// </summary>
    [Serializable]
    public abstract class ProcessCommand<TAggregateRootId> : Command<TAggregateRootId>, IProcessCommand
    {
        #region Public Properties

        /// <summary>Represents the process id.
        /// </summary>
        public string ProcessId { get; set; }

        #endregion

        #region Constructors

        /// <summary>Default constructor.
        /// </summary>
        protected ProcessCommand() : this(default(TAggregateRootId)) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        protected ProcessCommand(TAggregateRootId aggregateRootId) : this(aggregateRootId, DefaultRetryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="retryCount"></param>
        protected ProcessCommand(TAggregateRootId aggregateRootId, int retryCount)
            : base(aggregateRootId, retryCount)
        {
        }

        #endregion
    }
}
