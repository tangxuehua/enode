using System.Collections.Generic;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class ProcessingCommand
    {
        private ProcessingCommandMailbox _mailbox;

        public ICommand Message { get; private set; }
        public ICommandExecuteContext CommandExecuteContext { get; private set; }
        public int ConcurrentRetriedCount { get; private set; }
        public IDictionary<string, string> Items { get; private set; }

        public ProcessingCommand(ICommand command, ICommandExecuteContext commandExecuteContext, IDictionary<string, string> items)
        {
            Message = command;
            CommandExecuteContext = commandExecuteContext;
            Items = items ?? new Dictionary<string, string>();
        }

        public void SetMailbox(ProcessingCommandMailbox mailbox)
        {
            _mailbox = mailbox;
        }
        public void Complete()
        {
            if (_mailbox != null)
            {
                _mailbox.CompleteMessage(this);
            }
        }
        public void SetResult(CommandResult commandResult)
        {
            CommandExecuteContext.OnCommandExecuted(commandResult);
        }
        public void IncreaseConcurrentRetriedCount()
        {
            ConcurrentRetriedCount++;
        }
    }
}
