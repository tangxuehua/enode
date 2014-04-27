using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class CommandHandlerNotFoundException : ENodeException
    {
        private const string ExceptionMessage = "Command handler not found, commandType:{0}, commandId:{1}.";

        public CommandHandlerNotFoundException(ICommand command) : base(ExceptionMessage, command.GetType().FullName, command.Id) { }
    }
}
