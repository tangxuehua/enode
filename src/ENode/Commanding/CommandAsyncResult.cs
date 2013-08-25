using System;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents the command execution async result.
    /// </summary>
    public class CommandAsyncResult
    {
        private readonly ManualResetEvent _waitHandle;
        private readonly Action<CommandAsyncResult> _callback;

        /// <summary>Represents whether the command execution is completed.
        /// </summary>
        public bool IsCompleted { get; private set; }
        /// <summary>Represents the id of aggregate root which was created or updated by the command.
        /// <remarks>Can be null if the command not effect any aggregate root.</remarks>
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>Error message generated when executing the command.
        /// </summary>
        public ErrorInfo ErrorInfo { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="waitHandle"></param>
        public CommandAsyncResult(ManualResetEvent waitHandle)
        {
            if (waitHandle == null)
            {
                throw new ArgumentNullException("waitHandle");
            }
            _waitHandle = waitHandle;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="callback"></param>
        public CommandAsyncResult(Action<CommandAsyncResult> callback)
        {
            _callback = callback;
        }

        /// <summary>Complete the command execution async result.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="errorInfo"></param>
        public void Complete(string aggregateRootId, ErrorInfo errorInfo)
        {
            IsCompleted = true;
            AggregateRootId = aggregateRootId;
            ErrorInfo = errorInfo;

            if (_waitHandle != null)
            {
                _waitHandle.Set();
            }
            else if (_callback != null)
            {
                _callback(this);
            }
        }
    }
}
