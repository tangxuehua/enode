using System;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class DefaultCommandService : ICommandService
    {
        private ICommandQueueRouter _commandQueueRouter;
        private ICommandAsyncResultManager _commandAsyncResultManager;

        public DefaultCommandService(
            ICommandQueueRouter commandQueueRouter,
            ICommandAsyncResultManager commandAsyncResultManager)
        {
            _commandQueueRouter = commandQueueRouter;
            _commandAsyncResultManager = commandAsyncResultManager;
        }

        public void Send(ICommand command, Action<CommandAsyncResult> callback = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

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
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var commandQueue = _commandQueueRouter.Route(command);
            if (commandQueue == null)
            {
                throw new Exception("Could not route the command to an appropriate command queue.");
            }

            var waitHandle = new ManualResetEvent(false);
            var commandAsyncResult = new CommandAsyncResult(waitHandle);

            _commandAsyncResultManager.Add(command.Id, commandAsyncResult);
            commandQueue.Enqueue(command);
            waitHandle.WaitOne(command.MillisecondsTimeout);
            _commandAsyncResultManager.Remove(command.Id);

            if (!commandAsyncResult.IsCompleted)
            {
                throw new CommandTimeoutException(command.Id, command.GetType());
            }
            else if (commandAsyncResult.Exception != null)
            {
                throw new CommandExecuteException(command.Id, command.GetType(), commandAsyncResult.Exception);
            }
            else if (commandAsyncResult.ErrorMessage != null)
            {
                throw new CommandExecuteException(command.Id, command.GetType(), commandAsyncResult.ErrorMessage);
            }
        }
    }
}
