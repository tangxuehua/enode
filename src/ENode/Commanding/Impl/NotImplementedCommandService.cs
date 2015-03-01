using System;
using System.Threading.Tasks;
using ENode.Infrastructure;

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
        public Task<AsyncTaskResult> SendAsync(ICommand command)
        {
            throw new NotImplementedException();
        }
        public Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command)
        {
            throw new NotImplementedException();
        }
        public Task<AsyncTaskResult<CommandResult>> ExecuteAsync(ICommand command, CommandReturnType commandReturnType)
        {
            throw new NotImplementedException();
        }
    }
}
