using System.Collections.Generic;
using ENode.Commanding;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventContext.
    /// </summary>
    public class EventContext : IEventContext
    {
        private List<ICommand> _commands = new List<ICommand>();

        /// <summary>Add a to be execute command in the context.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }
        /// <summary>Get all the to be execute commands from the context.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICommand> GetCommands()
        {
            return _commands;
        }
    }
}
