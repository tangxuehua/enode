using System.Collections.Generic;

namespace ENode.Infrastructure
{
    /// <summary>Represents an exception which can be published.
    /// </summary>
    public interface IPublishableException
    {
        string UniqueId { get; }
        int TypeCode { get; }
        int ErrorCode { get; }
        object[] Arguments { get; }
        IDictionary<string, string> Items { get; }
    }
}
