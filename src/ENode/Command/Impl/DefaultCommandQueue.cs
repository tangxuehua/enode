using ENode.Messaging;

namespace ENode.Commanding
{
    public class DefaultCommandQueue : MessageQueue<ICommand>, ICommandQueue
    {
        public DefaultCommandQueue(string queueName) : base(queueName) { }
    }
}
