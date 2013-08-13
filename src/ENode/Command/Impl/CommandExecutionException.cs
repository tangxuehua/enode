using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when executing a command.
    /// </summary>
    [Serializable]
    public class CommandExecutionException : Exception
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="commandType"></param>
        /// <param name="errorMessage"></param>
        public CommandExecutionException(Guid commandId, Type commandType, string errorMessage) : this(commandId, commandType, errorMessage, null) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="commandType"></param>
        /// <param name="innerException"></param>
        public CommandExecutionException(Guid commandId, Type commandType, Exception innerException) : this(commandId, commandType, null, innerException) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="commandType"></param>
        /// <param name="errorMessage"></param>
        /// <param name="innerException"></param>
        public CommandExecutionException(Guid commandId, Type commandType, string errorMessage, Exception innerException)
            : base(string.Format("{0} execute error, command Id:{1}, error message:{2}", commandType.Name, commandId, errorMessage), innerException) { }
    }
}
