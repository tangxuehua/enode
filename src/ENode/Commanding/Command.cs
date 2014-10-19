using System;
using System.Collections.Generic;
using ECommon.Utilities;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class Command : ICommand
    {
        #region Private Variables

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
                    throw new Exception(string.Format("Command retry count cannot exceed {0}.", MaxRetryCount));
                }
                _retryCount = value;
            }
        }
        /// <summary>Represents the extension information of the command.
        /// </summary>
        public IDictionary<string, string> Items { get; private set; }

        #endregion

        #region Constructors

        /// <summary>Default constructor.
        /// </summary>
        protected Command()
        {
            Id = ObjectId.GenerateNewStringId();
            RetryCount = DefaultRetryCount;
            Items = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods

        /// <summary>Returns string.Empty by default.
        /// </summary>
        /// <returns></returns>
        public virtual object GetKey()
        {
            return string.Empty;
        }

        #endregion
    }
}
