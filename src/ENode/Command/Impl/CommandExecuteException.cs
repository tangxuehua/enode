using System;

namespace ENode.Commanding
{
    [Serializable]
    public class CommandExecuteException : Exception
    {
        public CommandExecuteException(Guid commandId, Type commandType, string errorMessage)
            : base(string.Format("{0} execute error, command Id:{1}, ex:{2}", commandType.Name, commandId, errorMessage)) { }

        public CommandExecuteException(Guid commandId, Type commandType, Exception innerException)
            : base(string.Format("{0} execute error, command Id:{1}", commandType.Name, commandId), innerException) { }
    }
}
