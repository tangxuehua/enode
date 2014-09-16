using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class CommandProcessIdMissingException : ENodeException
    {
        private const string ExceptionMessage = "ProcessId cannot be null or empty if the command is not a IStartProcessCommand, commandType:{0}, commandId:{1}.";

        public CommandProcessIdMissingException(ICommand command) : base(ExceptionMessage, command.GetType().FullName, command.Id) { }
    }
}
