using System.Threading.Tasks;
using ECommon.IO;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class CommandAsyncHandlerProxy<TCommand> : ICommandAsyncHandlerProxy where TCommand : class, ICommand
    {
        private readonly ICommandAsyncHandler<TCommand> _commandHandler;

        public CommandAsyncHandlerProxy(ICommandAsyncHandler<TCommand> commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(ICommand command)
        {
            return _commandHandler.HandleAsync(command as TCommand);
        }
        public object GetInnerObject()
        {
            return _commandHandler;
        }
    }
}
