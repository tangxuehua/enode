using System;
using System.Collections.Generic;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>Represents the unique identifier of the message.
        /// </summary>
        string Id { get; set; }
        /// <summary>Represents the timestamp of the message.
        /// </summary>
        DateTime Timestamp { get; set; }
        /// <summary>Represents the extension key/values data of the message.
        /// </summary>
        IDictionary<string, string> Items { get; set; }
        /// <summary>Merge the givens key/values into the current Items.
        /// </summary>
        /// <param name="items"></param>
        void MergeItems(IDictionary<string, string> items);
    }
}
