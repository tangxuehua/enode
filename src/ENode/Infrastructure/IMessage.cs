using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>Represents the unique identifier of the message.
        /// </summary>
        string Id { get; }
        /// <summary>Represents the timestamp of the message.
        /// </summary>
        DateTime Timestamp { get; }
        /// <summary>Set the id.
        /// </summary>
        /// <param name="id"></param>
        void SetId(string id);
        /// <summary>Represents the routing key of the message.
        /// </summary>
        /// <returns></returns>
        string GetRoutingKey();
    }
}
