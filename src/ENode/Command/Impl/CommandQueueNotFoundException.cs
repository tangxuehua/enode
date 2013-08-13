using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when the command queue cannot be routed.
    /// </summary>
    [Serializable]
    public class CommandQueueNotFoundException : Exception
    {
        private const string ExceptionMessage = "Cannot route an available command queue for command {0}.";

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        public CommandQueueNotFoundException(Type commandType) : base(string.Format(ExceptionMessage, commandType.Name)) { }
    }
}
