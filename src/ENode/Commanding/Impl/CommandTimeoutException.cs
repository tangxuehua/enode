using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when a command execution timeout.
    /// </summary>
    [Serializable]
    public class CommandTimeoutException : Exception
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="commandType"></param>
        public CommandTimeoutException(Guid commandId, Type commandType) : base(string.Format("Handle {0} timeout, command Id:{1}", commandType.Name, commandId)) { }
    }
}
