using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class DefaultCommandService : ICommandService
    {
        private ICommandQueueRouter _commandQueueRouter;
        private ICommandAsyncResultManager _commandAsyncResultManager;
        private ICommandExecutor _commandExecutor;

        public DefaultCommandService(ICommandQueueRouter commandQueueRouter, ICommandAsyncResultManager commandAsyncResultManager, ICommandExecutor commandExecutor)
        {
            _commandQueueRouter = commandQueueRouter;
            _commandAsyncResultManager = commandAsyncResultManager;
            _commandExecutor = commandExecutor;
        }

        public void Send(ICommand command, Action<CommandAsyncResult> callback = null)
        {
            var commandQueue = _commandQueueRouter.Route(command);
            if (commandQueue == null)
            {
                throw new Exception("Could not route the command to an appropriate command queue.");
            }

            if (callback != null)
            {
                _commandAsyncResultManager.Add(command.Id, new CommandAsyncResult(callback));
            }
            commandQueue.Enqueue(command);
        }
        public void Execute(ICommand command)
        {
            _commandExecutor.Execute(ObjectContainer.Resolve<ICommandContext>(), command);
        }
    }
}
