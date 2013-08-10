using ENode.Messaging;

namespace ENode.Commanding {
    public class DefaultCommandProcessor :
        MessageProcessor<ICommandQueue, ICommandExecutor, ICommand>,
        ICommandProcessor {
        public DefaultCommandProcessor(ICommandQueue bindingQueue, int workerCount = 1)
            : base(bindingQueue, workerCount) {
        }
    }
}
