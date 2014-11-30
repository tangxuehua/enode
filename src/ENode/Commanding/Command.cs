using System;
using ECommon.Utilities;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class Command : ICommand
    {
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        public string Id { get; set; }
        /// <summary>Represents the retry count of the command.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        protected Command()
        {
            Id = ObjectId.GenerateNewStringId();
            RetryCount = 3;
        }

        /// <summary>Returns string.Empty by default.
        /// </summary>
        /// <returns></returns>
        public virtual object GetKey()
        {
            return string.Empty;
        }
    }
}
