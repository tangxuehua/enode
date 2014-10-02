using System;
using ECommon.Utilities;

namespace ENode.Exceptions
{
    /// <summary>Represents a base publishable exception.
    /// </summary>
    public class PublishableException : Exception
    {
        public string UniqueId { get; set; }
        public string ProcessId { get; set; }

        public PublishableException()
        {
            UniqueId = ObjectId.GenerateNewStringId();
        }
    }
}
