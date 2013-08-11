using System;

namespace ENode.Commanding {
    [Serializable]
    public class CommandExecuteException : Exception {
        public CommandExecuteException(Guid commandId, Type commandType, string errorMessage) : this(commandId, commandType, errorMessage, null) { }
        public CommandExecuteException(Guid commandId, Type commandType, Exception innerException) : this(commandId, commandType, null, innerException) { }
        public CommandExecuteException(Guid commandId, Type commandType, string errorMessage, Exception innerException)
            : base(string.Format("{0} execute error, command Id:{1}, error message:{2}", commandType.Name, commandId, errorMessage), innerException) { }
    }
}
