using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class CommandAggregateRootIdMissingException : ENodeException
    {
        private const string ExceptionMessage = "AggregateRootId cannot be null if the command is not a ICreatingAggregateCommand, commandType:{0}, commandId:{1}.";

        public CommandAggregateRootIdMissingException(ICommand command) : base(ExceptionMessage, command.GetType().FullName, command.Id) { }
    }
}
