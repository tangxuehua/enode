using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class CommandAggregateRootIdMissingException : Exception
    {
        private const string ExceptionMessage = "AggregateRootId cannot be null if the command is not a ICreatingAggregateCommand, commandType:{0}, commandId:{1}.";

        public CommandAggregateRootIdMissingException(ICommand command) : base(string.Format(ExceptionMessage, command.GetType().FullName, command.Id)) { }
    }
}
