using System;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Commanding.Impl
{
    public class NotImplementedCommandService : ICommandService
    {
        public void Send(ICommand command)
        {
            throw new NotImplementedException();
        }
        public Task<AsyncTaskResult> SendAsync(ICommand command)
        {
            throw new NotImplementedException();
        }
        public CommandResult Execute(ICommand command, int timeoutMillis)
        {
            throw new NotImplementedException();
        }
        public CommandResult Execute(ICommand command, CommandReturnType commandReturnType, int timeoutMillis)
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
