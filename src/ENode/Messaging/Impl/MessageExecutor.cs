using System.Collections.Generic;

namespace ENode.Messaging {
    public abstract class MessageExecutor<TMessage> : IMessageExecutor<TMessage> where TMessage : class, IMessage {
        public abstract void Execute(TMessage message, IMessageQueue<TMessage> queue);
        /// <summary>Finish the message execution, the message will be removed from the message queue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        protected virtual void FinishExecution(TMessage message, IMessageQueue<TMessage> queue) {
            queue.Complete(message);
        }
    }
}
