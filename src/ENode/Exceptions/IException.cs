using System.Collections.Generic;

namespace ENode.Exceptions
{
    /// <summary>Represents an exception which can be published.
    /// </summary>
    public interface IException
    {
        string UniqueId { get; }
        string Message { get; }
        IDictionary<string, string> Items { get; }
    }
}
