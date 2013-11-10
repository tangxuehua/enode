using System;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandService.
    /// </summary>
    public class DefaultCommandService : ICommandService
    {
        private readonly ICommandQueueRouter _commandQueueRouter;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandQueueRouter"></param>
        public DefaultCommandService(ICommandQueueRouter commandQueueRouter)
        {
            _commandQueueRouter = commandQueueRouter;
        }

        /// <summary>Send the command to a specific command queue.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <exception cref="ArgumentNullException">Throwed when the command is null.</exception>
        /// <exception cref="CommandQueueNotFoundException">Throwed when the command queue cannot be routed.</exception>
        public void Send(ICommand command)
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

            commandQueue.Enqueue(command);
        }
    }
}
