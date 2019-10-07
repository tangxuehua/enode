using System.Collections.Generic;
using ENode.Messaging;

namespace ENode.Domain
{
    /// <summary>Represents a domain exception which is raised from domain layer.
    /// </summary>
    public interface IDomainException : IMessage
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
