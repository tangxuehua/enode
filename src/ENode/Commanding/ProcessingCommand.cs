using System.Collections.Generic;

namespace ENode.Commanding
{
    public class ProcessingCommand
    {
        public ProcessingCommandMailbox Mailbox { get; set; }
        public long Sequence { get; set; }
        public ICommand Message { get; private set; }
        public ICommandExecuteContext CommandExecuteContext { get; private set; }
        public IDictionary<string, string> Items { get; private set; }

        public ProcessingCommand(ICommand command, ICommandExecuteContext commandExecuteContext, IDictionary<string, string> items)
        {
            Message = command;
            CommandExecuteContext = commandExecuteContext;
            Items = items ?? new Dictionary<string, string>();
        }

        public void Complete(CommandResult commandResult)
        {
            CommandExecuteContext.OnCommandExecuted(commandResult);
        }
    }
}
