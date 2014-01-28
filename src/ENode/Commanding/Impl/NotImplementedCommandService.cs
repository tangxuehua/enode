using System;
using System.Threading.Tasks;

namespace ENode.Commanding.Impl
{
    public class NotImplementedCommandService : ICommandService
    {
        public Task<CommandResult> Send(ICommand command)
        {
            throw new NotImplementedException("NotImplementedCommandService does not support sending command.");
        }
    }
}
