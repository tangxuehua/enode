using System.Collections.Concurrent;
using ECommon.Logging;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandProcessor : ICommandProcessor
    {
        private readonly ConcurrentDictionary<string, CommandMailbox> _mailboxDict;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILoggerFactory _loggerFactory;

        public DefaultCommandProcessor(ICommandDispatcher commandDispatcher, ICommandExecutor commandExecutor, ILoggerFactory loggerFactory)
        {
            _mailboxDict = new ConcurrentDictionary<string, CommandMailbox>();
            _commandDispatcher = commandDispatcher;
            _commandExecutor = commandExecutor;
            _loggerFactory = loggerFactory;
        }

        public void Process(ProcessingCommand processingCommand)
        {
            if (string.IsNullOrEmpty(processingCommand.AggregateRootId))
            {
                _commandDispatcher.RegisterCommandForExecution(processingCommand);
            }
            else
            {
                var commandMailbox = _mailboxDict.GetOrAdd(processingCommand.AggregateRootId, new CommandMailbox(_commandDispatcher, _commandExecutor, _loggerFactory));
                commandMailbox.EnqueueCommand(processingCommand);
                _commandDispatcher.RegisterMailboxForExecution(commandMailbox);
            }
        }
    }
}
