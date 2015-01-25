using System.Collections.Generic;
using ENode.Infrastructure;

namespace ENode.Exceptions
{
    /// <summary>Represents an exception which can be published.
    /// </summary>
    public interface IPublishableException : IDispatchableMessage
    {
        /// <summary>Represents the uniqueId of the exception.
        /// </summary>
        string UniqueId { get; set; }
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
