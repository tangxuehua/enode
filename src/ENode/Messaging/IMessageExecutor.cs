using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message executor.
    /// </summary>
    public interface IMessageExecutor<T> where T : IMessage
    {
        /// <summary>Execute the given queue message.
        /// <remarks>
        /// The message executor will determine that whether the message was execute successfully,
        /// If the message was executed successfully, then return MessageExecuteResult.Executed; otherwise, return MessageExecuteResult.Failed;
        /// If return MessageExecuteResult.Failed, then the message will be re-executed again for 3 times by default.
        /// If still return MessageExecuteResult.Failed, then the message will be put to a local retry queue within the messag processor,
        /// and a worker thread will execute the message later with some interval repeatedly until the message was executed successfully.
        /// </remarks>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        MessageExecuteResult Execute(T message);
    }
}
