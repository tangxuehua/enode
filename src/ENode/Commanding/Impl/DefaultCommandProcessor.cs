using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandProcessor : ICommandProcessor
    {
        private readonly ParallelProcessor<ProcessingCommand> _parallelProcessor;
        private readonly ICommandExecutor _commandExecutor;

        public DefaultCommandProcessor(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
            _parallelProcessor = new ParallelProcessor<ProcessingCommand>(
                ENodeConfiguration.Instance.Setting.CommandProcessorParallelThreadCount,
                "ProcessCommand",
                x => _commandExecutor.ExecuteCommand(x));
        }

        public void Start()
        {
            _parallelProcessor.Start();
        }
        public void Process(ProcessingCommand processingCommand)
        {
            _parallelProcessor.EnqueueMessage(processingCommand.GetRoutingKey(), processingCommand);
        }
    }
}
