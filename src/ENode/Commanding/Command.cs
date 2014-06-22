using System;
using System.Collections.Generic;
using ECommon.Utilities;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class Command<TAggregateRootId> : ICommand
    {
        #region Private Variables

        private TAggregateRootId _aggregateRootId;
        private string _aggregateRootStringId;
        private int _retryCount;
        public const int DefaultRetryCount = 3;
        public const int MaxRetryCount = 10;

        #endregion

        #region Public Properties

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
        /// <summary>Represents the extension information of the command.
        /// </summary>
        public IDictionary<string, string> Items { get; private set; }

        #endregion

        #region Constructors

        /// <summary>Default constructor.
        /// </summary>
        protected Command() : this(default(TAggregateRootId)) { }
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
            Id = ObjectId.GenerateNewStringId();
            _aggregateRootId = aggregateRootId;
            if (aggregateRootId != null)
            {
                _aggregateRootStringId = aggregateRootId.ToString();
            }
            RetryCount = retryCount;
            Items = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods

        /// <summary>Returns the aggregate root id as the key by default.
        /// </summary>
        /// <returns></returns>
        public virtual object GetKey()
        {
            return _aggregateRootId;
        }

        #endregion
    }
}
