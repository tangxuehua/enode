using System;
using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract base command.
    /// </summary>
    [Serializable]
    public abstract class Command : Message, ICommand
    {
        private int _retryCount;
        private const int DefaultMillisecondsTimeout = 10000;
        private const int DefaultRetryCount = 3;
        private const int MaxRetryCount = 5;

        /// <summary>Get or set command executing waiting milliseconds.
        /// </summary>
        public int MillisecondsTimeout { get; set; }
        /// <summary>Get or set times which the command should be retry. The retry count must small than 5;
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
                    throw new Exception(string.Format("Command max retry count cannot exceed {0}.", MaxRetryCount));
                }
                _retryCount = value;
            }
        }

        /// <summary>Default constructor.
        /// </summary>
        protected Command() : this(DefaultMillisecondsTimeout, DefaultRetryCount) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="retryCount"></param>
        protected Command(int millisecondsTimeout, int retryCount) : base(Guid.NewGuid())
        {
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
