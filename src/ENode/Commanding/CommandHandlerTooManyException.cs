using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class CommandHandlerTooManyException : Exception
    {
        private const string ExceptionMessage = "Found more than one command handler, commandType:{0}, commandId:{1}.";

        public CommandHandlerTooManyException(ICommand command) : base(string.Format(ExceptionMessage, command.GetType().FullName, command.Id)) { }
    }
}
