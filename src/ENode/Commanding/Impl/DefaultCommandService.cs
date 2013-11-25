using System;
using System.Threading.Tasks;
using ENode.Messaging;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandService.
    /// </summary>
    public class DefaultCommandService : ICommandService
    {
        private readonly ICommandQueueRouter _commandQueueRouter;
        private readonly ICommandTaskManager _commandTaskManager;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandQueueRouter"></param>
        public DefaultCommandService(ICommandQueueRouter commandQueueRouter, ICommandTaskManager commandTaskManager)
        {
            _commandQueueRouter = commandQueueRouter;
            _commandTaskManager = commandTaskManager;
        }

        /// <summary>Send the command to a specific command queue and returns a task object.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <exception cref="ArgumentNullException">Throwed when the command is null.</exception>
        /// <exception cref="CommandQueueNotFoundException">Throwed when the command queue cannot be routed.</exception>
        public Task<CommandResult> Send(ICommand command)
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

            var task = _commandTaskManager.CreateCommandTask(command.Id);
            commandQueue.Enqueue(new Message<ICommand>(Guid.NewGuid(), command, commandQueue.Name));
            return task;
        }
    }
}
