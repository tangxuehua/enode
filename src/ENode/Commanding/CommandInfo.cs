using System;

namespace ENode.Commanding
{
    /// <summary>This class contains a command and its retried count info.
    /// </summary>
    public class CommandInfo
    {
        /// <summary>The command.
        /// </summary>
        public ICommand Command { get; private set; }
        /// <summary>The retry count of command.
        /// </summary>
        public int RetriedCount { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="command"></param>
        public CommandInfo(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            Command = command;
        }

        /// <summary>Increase the command retried count.
        /// </summary>
        public void IncreaseRetriedCount()
        {
            RetriedCount++;
        }
    }
}
