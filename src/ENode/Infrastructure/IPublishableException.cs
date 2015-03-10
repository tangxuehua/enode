using System.Collections.Generic;

namespace ENode.Infrastructure
{
    /// <summary>Represents an exception which can be published.
    /// </summary>
    public interface IPublishableException : IMessage
    {
        /// <summary>Serialize the current exception info to the given dictionary.
        /// </summary>
        /// <param name="serializableInfo"></param>
        void SerializeTo(IDictionary<string, string> serializableInfo);
        /// <summary>Restore the current exception from the given dictionary.
        /// </summary>
        /// <param name="serializableInfo"></param>
        void RestoreFrom(IDictionary<string, string> serializableInfo);
    }
}
