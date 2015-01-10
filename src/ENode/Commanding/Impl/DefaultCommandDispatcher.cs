using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Extensions;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandDispatcher : ICommandDispatcher
    {
        private readonly TaskFactory _taskFactory;
        private readonly ICommandExecutor _commandExecutor;

        public DefaultCommandDispatcher(ICommandExecutor commandExecutor)
        {
            var setting = ENodeConfiguration.Instance.Setting;
            _taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(setting.CommandProcessorParallelThreadCount));
            _commandExecutor = commandExecutor;
        }

        public void RegisterCommandForExecution(ProcessingCommand command)
        {
            _taskFactory.StartNew(() => _commandExecutor.ExecuteCommand(command));
        }
        public void RegisterMailboxForExecution(CommandMailbox mailbox)
        {
            _taskFactory.StartNew(() => TryRunMailbox(mailbox));
        }

        private void TryRunMailbox(CommandMailbox mailbox)
        {
            if (mailbox.MarkAsRunning())
            {
                _taskFactory.StartNew(mailbox.Run);
            }
        }
    }
}
