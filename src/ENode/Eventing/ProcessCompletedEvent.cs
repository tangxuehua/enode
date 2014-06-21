using System;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract base process completed domain event.
    /// </summary>
    [Serializable]
    public abstract class ProcessCompletedEvent<TAggregateRootId> : DomainEvent<TAggregateRootId>, IProcessCompletedEvent
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        public ProcessCompletedEvent(TAggregateRootId aggregateRootId) : base(aggregateRootId)
        {
            IsSuccess = true;
            ErrorCode = 0;
        }

        /// <summary>Represents whether the process is success.
        /// </summary>
        public bool IsSuccess
        {
            get;
            protected set;
        }
        /// <summary>Represents the error code if the process is not success.
        /// </summary>
        public int ErrorCode
        {
            get;
            protected set;
        }
    }
}
