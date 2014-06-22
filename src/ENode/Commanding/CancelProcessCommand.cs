using System;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract cancel process command.
    /// </summary>
    [Serializable]
    public abstract class CancelProcessCommand<TAggregateRootId> : ProcessCommand<TAggregateRootId>
    {
        /// <summary>Represents the error code of the process.
        /// </summary>
        public int ErrorCode { get; protected set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="errorCode"></param>
        protected CancelProcessCommand(object processId, int errorCode)
            : base(processId)
        {
            ErrorCode = errorCode;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="errorCode"></param>
        protected CancelProcessCommand(object processId, TAggregateRootId aggregateRootId, int errorCode)
            : base(processId, aggregateRootId)
        {
            ErrorCode = errorCode;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="retryCount"></param>
        /// <param name="errorCode"></param>
        protected CancelProcessCommand(object processId, TAggregateRootId aggregateRootId, int retryCount, int errorCode)
            : base(processId, aggregateRootId, retryCount)
        {
            ErrorCode = errorCode;
        }
    }
}
