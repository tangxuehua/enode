using ENode.Messaging;

namespace ENode.Commanding
{
    public class DefaultCommandQueue : QueueBase<ICommand>, ICommandQueue
    {
        public DefaultCommandQueue(string queueName) : base(queueName) { }
    }
}
