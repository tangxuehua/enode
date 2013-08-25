using System;
using System.Threading;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandService.
    /// </summary>
    public class DefaultCommandService : ICommandService
    {
        private readonly ICommandQueueRouter _commandQueueRouter;
        private readonly ICommandAsyncResultManager _commandAsyncResultManager;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandQueueRouter"></param>
        /// <param name="commandAsyncResultManager"></param>
        public DefaultCommandService(ICommandQueueRouter commandQueueRouter, ICommandAsyncResultManager commandAsyncResultManager)
        {
            _commandQueueRouter = commandQueueRouter;
            _commandAsyncResultManager = commandAsyncResultManager;
        }

        /// <summary>Send the given command to command queue and return immediately, the command will be handle asynchronously.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="callback">The callback method when the command was handled.</param>
        /// <exception cref="ArgumentNullException">Throwed when the command is null.</exception>
        /// <exception cref="CommandQueueNotFoundException">Throwed when the command queue cannot be routed.</exception>
        public void Send(ICommand command, Action<CommandAsyncResult> callback = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var commandQueue = _commandQueueRouter.Route(command);
            if (commandQueue == null)
            {
                throw new CommandQueueNotFoundException(command.GetType());
            }

            if (callback != null)
            {
                _commandAsyncResultManager.Add(command.Id, new CommandAsyncResult(callback));
            }
            commandQueue.Enqueue(command);
        }
        /// <summary>Send the given command to command queue, and block the current thread until the command was handled or timeout.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns>The command execution result.</returns>
        /// <exception cref="ArgumentNullException">Throwed when the command is null.</exception>
        /// <exception cref="CommandQueueNotFoundException">Throwed when the command queue cannot be routed.</exception>
        /// <exception cref="CommandTimeoutException">Throwed when the command execution timeout.</exception>
        /// <exception cref="CommandExecutionException">Throwed when the command execution has any error.</exception>
        public CommandAsyncResult Execute(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var commandQueue = _commandQueueRouter.Route(command);
            if (commandQueue == null)
            {
                throw new CommandQueueNotFoundException(command.GetType());
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
            if (commandAsyncResult.ErrorInfo == null)
            {
                return commandAsyncResult;
            }

            var errorInfo = commandAsyncResult.ErrorInfo;
            if (errorInfo.ErrorMessage != null && errorInfo.Exception != null)
            {
                throw new CommandExecutionException(command.Id, command.GetType(), errorInfo.ErrorMessage, errorInfo.Exception);
            }
            if (errorInfo.ErrorMessage != null)
            {
                throw new CommandExecutionException(command.Id, command.GetType(), errorInfo.ErrorMessage);
            }
            if (errorInfo.Exception != null)
            {
                throw new CommandExecutionException(command.Id, command.GetType(), errorInfo.Exception);
            }

            return commandAsyncResult;
        }
    }
}
