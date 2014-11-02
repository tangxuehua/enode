using System.Collections.Generic;

namespace ENode.Exceptions
{
    /// <summary>Represents an exception which can be published.
    /// </summary>
    public interface IPublishableException
    {
        string UniqueId { get; set; }
        void SerializeTo(IDictionary<string, string> serializableInfo);
        void RestoreFrom(IDictionary<string, string> serializableInfo);
    }
}
