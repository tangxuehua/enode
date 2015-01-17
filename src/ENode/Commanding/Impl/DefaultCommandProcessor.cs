using System.Collections.Concurrent;
using ECommon.Logging;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandProcessor : ICommandProcessor
    {
        private readonly ConcurrentDictionary<string, CommandMailbox> _mailboxDict;
        private readonly ICommandScheduler _commandScheduler;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILoggerFactory _loggerFactory;

        public DefaultCommandProcessor(ICommandScheduler commandScheduler, ICommandExecutor commandExecutor, ILoggerFactory loggerFactory)
        {
            _mailboxDict = new ConcurrentDictionary<string, CommandMailbox>();
            _commandScheduler = commandScheduler;
            _commandExecutor = commandExecutor;
            _loggerFactory = loggerFactory;
        }

        public void Process(ProcessingCommand processingCommand)
        {
            if (string.IsNullOrEmpty(processingCommand.AggregateRootId))
            {
                _commandScheduler.ScheduleCommand(processingCommand);
            }
            else
            {
                var commandMailbox = _mailboxDict.GetOrAdd(processingCommand.AggregateRootId, new CommandMailbox(_commandScheduler, _commandExecutor, _loggerFactory));
                commandMailbox.EnqueueCommand(processingCommand);
                _commandScheduler.ScheduleCommandMailbox(commandMailbox);
            }
        }
    }
}
