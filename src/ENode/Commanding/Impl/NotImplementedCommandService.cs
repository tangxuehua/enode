using System;
using System.Threading.Tasks;

namespace ENode.Commanding.Impl
{
    public class NotImplementedCommandService : ICommandService
    {
        public void Send(ICommand command)
        {
            throw new NotImplementedException();
        }
        public void Send(ICommand command, string sourceId, string sourceType)
        {
            throw new NotImplementedException();
        }
        public Task<CommandSendResult> SendAsync(ICommand command)
        {
            throw new NotImplementedException();
        }
        public Task<CommandResult> Execute(ICommand command)
        {
            throw new NotImplementedException();
        }
        public Task<CommandResult> Execute(ICommand command, CommandReturnType commandReturnType)
        {
            throw new NotImplementedException();
        }
    }
}
