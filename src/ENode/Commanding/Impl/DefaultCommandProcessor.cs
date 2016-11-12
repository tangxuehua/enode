using System;
using System.Collections.Concurrent;
using ECommon.Logging;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandProcessor : ICommandProcessor
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ProcessingCommandMailbox> _mailboxDict;
        private readonly IProcessingCommandHandler _handler;

        public DefaultCommandProcessor(IProcessingCommandHandler handler, ILoggerFactory loggerFactory)
        {
            _mailboxDict = new ConcurrentDictionary<string, ProcessingCommandMailbox>();
            _handler = handler;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public void Process(ProcessingCommand processingCommand)
        {
            var aggregateRootId = processingCommand.Message.AggregateRootId;
            if (string.IsNullOrEmpty(aggregateRootId))
            {
                throw new ArgumentException("aggregateRootId of command cannot be null or empty, commandId:" + processingCommand.Message.Id);
            }

            var mailbox = _mailboxDict.GetOrAdd(aggregateRootId, x =>
            {
                return new ProcessingCommandMailbox(x, _handler, _logger);
            });
            mailbox.EnqueueMessage(processingCommand);
        }
    }
}
