using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class Command<TAggregateRootId> : ICommand
    {
        private int _retryCount;
        public const int DefaultRetryCount = 5;
        public const int MaxRetryCount = 50;

        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Get or set the count which the command should be retry. The retry count must small than the MaxRetryCount;
        /// </summary>
        public int RetryCount
        {
            get
            {
                return _retryCount;
            }
            set
            {
                if (value > MaxRetryCount)
                {
                    throw new ENodeException("Command retry count cannot exceed {0}.", MaxRetryCount);
                }
                _retryCount = value;
            }
        }
        /// <summary>Represents the id of aggregate root which is created or updated by the command.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; private set; }
        /// <summary>Represents the id of the aggregate root, this property is only used by framework.
        /// </summary>
        string ICommand.AggregateRootId
        {
            get { return this.AggregateRootId.ToString(); }
        }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        protected Command(TAggregateRootId aggregateRootId) : this(aggregateRootId, DefaultRetryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="retryCount"></param>
        protected Command(TAggregateRootId aggregateRootId, int retryCount)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            Id = Guid.NewGuid();
            AggregateRootId = aggregateRootId;
            RetryCount = retryCount;
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
