using System;

namespace ENode.Messaging
{
    /// <summary>Represents a enum of queue message execution result.
    /// </summary>
    public enum MessageExecuteResult
    {
        /// <summary>Indicate that the queue message has not been executed yet.
        /// </summary>
        None,
        /// <summary>Indicate that the queue message has been executed,
        /// so that the message processor not need to retry executing the message again.
        /// </summary>
        Executed,
        /// <summary>Indicate that the queue message was executed failed,
        /// so that the message processor need to retry executing the message again.
        /// </summary>
        Failed
    }
}
