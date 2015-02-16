using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message publish result.
    /// </summary>
    [Serializable]
    public class PublishResult<TMessage> where TMessage : class
    {
        /// <summary>Represents the publish result status.
        /// </summary>
        public PublishStatus Status { get; private set; }
        /// <summary>Represents the error message if publish message is failed.
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>Represents the message to publish.
        /// </summary>
        public TMessage Message { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        public PublishResult(PublishStatus status, string errorMessage, TMessage message)
        {
            Status = status;
            ErrorMessage = errorMessage;
            Message = message;
        }
    }
    /// <summary>Represents the message publish result status enum.
    /// </summary>
    public enum PublishStatus
    {
        None = 0,
        Success = 1,
        IOException = 2,
        Failed = 3
    }
}
