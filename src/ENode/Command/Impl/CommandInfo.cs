using System;

namespace ENode.Commanding
{
    /// <summary>This class contains a command and its retried count info.
    /// </summary>
    public class CommandInfo
    {
        public ICommand Command { get; private set; }
        public int RetriedCount { get; private set; }

        public CommandInfo(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            Command = command;
        }

        public void IncreaseRetriedCount()
        {
            RetriedCount++;
        }
    }
}
