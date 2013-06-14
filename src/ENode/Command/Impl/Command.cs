using System;

namespace ENode.Commanding
{
    /// <summary>Represents a base command.
    /// </summary>
    [Serializable]
    public abstract class Command : ICommand
    {
        private int _retryCount;
        private const int DefaultMillisecondsTimeout = 10000;
        private const int DefaultRetryCount = 3;
        private const int MaxRetryCount = 5;

        /// <summary>The unique identifier of the command.
        /// </summary>
        public Guid Id { get; private set; }
        public int MillisecondsTimeout { get; set; }
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
                    throw new Exception(string.Format("Command max retry count cannot exceed {0}.", MaxRetryCount));
                }
                _retryCount = value;
            }
        }

        public Command() : this(DefaultMillisecondsTimeout, DefaultRetryCount) { }
        public Command(int millisecondsTimeout, int retryCount) : base()
        {
            Id = Guid.NewGuid();
            MillisecondsTimeout = millisecondsTimeout;
            RetryCount = retryCount;
        }
    }
}
