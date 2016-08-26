using System;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class CommandAsyncHandlerProxy<TCommand> : ICommandAsyncHandlerProxy where TCommand : class, ICommand
    {
        private readonly ICommandAsyncHandler<TCommand> _commandHandler;
        private readonly Type _commandHandlerType;

        public CommandAsyncHandlerProxy(ICommandAsyncHandler<TCommand> commandHandler, Type commandHandlerType)
        {
            _commandHandler = commandHandler;
            _commandHandlerType = commandHandlerType;
        }

        public Task<AsyncTaskResult<IApplicationMessage>> HandleAsync(ICommand command)
        {
            var handler = GetInnerObject() as ICommandAsyncHandler<TCommand>;
            return handler.HandleAsync(command as TCommand);
        }
        public object GetInnerObject()
        {
            if (_commandHandler != null)
            {
                return _commandHandler;
            }
            return ObjectContainer.Resolve(_commandHandlerType);
        }
    }
}
