using System;
using System.Collections.Generic;
using ECommon.Utilities;

namespace ENode.Exceptions
{
    public abstract class PublishableException : Exception, IPublishableException
    {
        public string Id { get { return UniqueId; } }
        public string UniqueId { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public PublishableException()
        {
            UniqueId = ObjectId.GenerateNewStringId();
        }

        public abstract void SerializeTo(IDictionary<string, string> serializableInfo);
        public abstract void RestoreFrom(System.Collections.Generic.IDictionary<string, string> serializableInfo);
    }
}
