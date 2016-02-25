using System.Threading.Tasks;

namespace ENode.Commanding.Impl
{
    public class DefaultProcessingCommandScheduler : IProcessingCommandScheduler
    {
        public void ScheduleMailbox(ProcessingCommandMailbox mailbox)
        {
            if (mailbox.EnterHandlingMessage())
            {
                Task.Factory.StartNew(mailbox.Run);
            }
        }
    }
}
