using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class CommandHandlerNotFoundException : Exception
    {
        private const string ExceptionMessage = "Command handler not found, commandType:{0}, commandId:{1}.";

        public CommandHandlerNotFoundException(ICommand command) : base(string.Format(ExceptionMessage, command.GetType().FullName, command.Id)) { }
    }
}
