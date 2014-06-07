using System;
using ECommon.Utilities;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class Command<TAggregateRootId> : ICommand
    {
        private TAggregateRootId _aggregateRootId;
        private string _aggregateRootStringId;
        private int _retryCount;
        public const int DefaultRetryCount = 5;
        public const int MaxRetryCount = 50;

        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        public string Id { get; set; }
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
        public TAggregateRootId AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }
        /// <summary>Represents the id of the aggregate root, this property is only used by framework.
        /// </summary>
        string ICommand.AggregateRootId
        {
            get
            {
                if (_aggregateRootStringId == null && _aggregateRootId != null)
                {
                    _aggregateRootStringId = _aggregateRootId.ToString();
                }
                return _aggregateRootStringId;
            }
        }

        /// <summary>Default constructor.
        /// </summary>
        /// <param name="commandId"></param>
        protected Command(string commandId) : this(commandId, DefaultRetryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="retryCount"></param>
        protected Command(string commandId, int retryCount) : this(commandId, default(TAggregateRootId), retryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="aggregateRootId"></param>
        protected Command(string commandId, TAggregateRootId aggregateRootId) : this(commandId, aggregateRootId, DefaultRetryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="retryCount"></param>
        protected Command(string commandId, TAggregateRootId aggregateRootId, int retryCount)
        {
            Id = commandId;
            _aggregateRootId = aggregateRootId;
            if (aggregateRootId != null)
            {
                _aggregateRootStringId = aggregateRootId.ToString();
            }
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
