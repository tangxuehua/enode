using System;
using System.Collections.Generic;
using ECommon.Utilities;

namespace ENode.Infrastructure
{
    public abstract class PublishableException : Exception, IPublishableException
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }

        public PublishableException()
        {
            Id = ObjectId.GenerateNewStringId();
            Timestamp = DateTime.UtcNow;
        }

        public abstract void SerializeTo(IDictionary<string, string> serializableInfo);
        public abstract void RestoreFrom(IDictionary<string, string> serializableInfo);
    }
}
