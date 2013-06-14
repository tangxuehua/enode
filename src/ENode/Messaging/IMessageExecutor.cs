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
        /// If the message was executed successful, then return true; otherwise, return false;
        /// If return false, then the message will be re-executed again for 3 times by default.
        /// If still return false, then the message will be put to a queue within the messag processor,
        /// and a worker thread will execute the message later with some interval.
        /// </remarks>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool Execute(T message);
    }
}
