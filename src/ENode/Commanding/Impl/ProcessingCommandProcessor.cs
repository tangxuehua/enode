using System.Collections.Concurrent;
using ECommon.Scheduling;

namespace ENode.Commanding.Impl
{
    /// <summary>A processor used to accept and process ProcessingCommand.
    /// </summary>
    public class ProcessingCommandProcessor
    {
        private readonly BlockingCollection<ProcessingCommand> _queue;
        private readonly Worker _worker;
        private ICommandExecutor _commandExecutor;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="commandExecutor"></param>
        public ProcessingCommandProcessor()
        {
            _queue = new BlockingCollection<ProcessingCommand>(new ConcurrentQueue<ProcessingCommand>());
            _worker = new Worker(() => _commandExecutor.Execute(_queue.Take()));
        }

        /// <summary>Set the command executor.
        /// </summary>
        public void SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }
        /// <summary>Start the processing command processor.
        /// </summary>
        public void Start()
        {
            _worker.Start();
        }

        /// <summary>Add a processing command into processing command queue.
        /// </summary>
        /// <param name="processingCommand"></param>
        public void AddProcessingCommand(ProcessingCommand processingCommand)
        {
            _queue.Add(processingCommand);
        }
    }
}
