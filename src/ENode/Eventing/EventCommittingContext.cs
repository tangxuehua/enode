using ENode.Commanding;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class EventCommittingContext
    {
        public EventStream EventStream { get; private set; }
        public ICommand Command { get; private set; }
        public ICommandExecuteContext CommandExecuteContext { get; private set; }
        public ConcurrentException ConcurrentException { get; set; }

        public EventCommittingContext(EventStream eventStream, ICommand command, ICommandExecuteContext commandExecuteContext)
        {
            EventStream = eventStream;
            Command = command;
            CommandExecuteContext = commandExecuteContext;
        }
    }
}
