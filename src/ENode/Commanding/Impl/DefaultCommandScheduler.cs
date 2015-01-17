using System.Threading.Tasks;
using ECommon.Scheduling;
using ENode.Configurations;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandScheduler : ICommandScheduler
    {
        private readonly TaskFactory _taskFactory;
        private readonly ICommandExecutor _commandExecutor;

        public DefaultCommandScheduler(ICommandExecutor commandExecutor)
        {
            var setting = ENodeConfiguration.Instance.Setting;
            _taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(setting.CommandProcessorParallelThreadCount));
            _commandExecutor = commandExecutor;
        }

        public void ScheduleCommand(ProcessingCommand command)
        {
            _taskFactory.StartNew(() => _commandExecutor.ExecuteCommand(command));
        }
        public void ScheduleCommandMailbox(CommandMailbox mailbox)
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
