using System;
using System.Threading.Tasks;
using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class ProcessingCommandMailbox : AggregateMessageMailBox<ProcessingCommand, CommandResult>
    {
        public ProcessingCommandMailbox(string aggregateRootId, IProcessingCommandHandler messageHandler, ILogger logger)
            : base(aggregateRootId, ENodeConfiguration.Instance.Setting.CommandMailBoxPersistenceMaxBatchSize, false, (x => messageHandler.Handle(x)), null, logger)
        {

        }

        protected override Task CompleteMessageWithResult(ProcessingCommand processingCommand, CommandResult commandResult)
        {
            try
            {
                return processingCommand.CompleteAsync(commandResult);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to complete command, commandId: {0}, aggregateRootId: {1}", processingCommand.Message.Id, processingCommand.Message.AggregateRootId), ex);
                return Task.CompletedTask;
            }
        }
    }
}
