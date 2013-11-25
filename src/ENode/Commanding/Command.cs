using System;
using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract base command.
    /// </summary>
    [Serializable]
    public abstract class Command : ICommand
    {
        private object _aggregateRootId;
        private int _retryCount;
        public const int DefaultMillisecondsTimeout = 10000;
        public const int DefaultRetryCount = 5;
        public const int MaxRetryCount = 50;

        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Represents the id of aggregate root which will be created or updated by the current command.
        /// </summary>
        object ICommand.AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }
        /// <summary>Get or set command executing waiting milliseconds.
        /// </summary>
        public int MillisecondsTimeout { get; set; }
        /// <summary>Get or set times which the command should be retry. The retry count must small than the MaxRetryCount;
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
                    throw new Exception(string.Format("Command retry count cannot exceed {0}.", MaxRetryCount));
                }
                _retryCount = value;
            }
        }

        /// <summary>Default constructor.
        /// </summary>
        protected Command() : this(null, DefaultMillisecondsTimeout, DefaultRetryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        protected Command(object aggregateRootId) : this(aggregateRootId, DefaultMillisecondsTimeout, DefaultRetryCount)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="retryCount"></param>
        protected Command(int millisecondsTimeout, int retryCount) : this(null, millisecondsTimeout, retryCount)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="retryCount"></param>
        protected Command(object aggregateRootId, int millisecondsTimeout, int retryCount)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            if (millisecondsTimeout < 0)
            {
                throw new ArgumentException("Command millisecondsTimeout cannot be small than zero.");
            }
            Id = Guid.NewGuid();
            _aggregateRootId = aggregateRootId;
            MillisecondsTimeout = millisecondsTimeout;
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
