using System;
using System.Collections.Generic;
using ECommon.Utilities;

namespace ENode.Infrastructure
{
    public abstract class PublishableException : Exception, IPublishableException
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int Sequence { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public PublishableException()
        {
            Id = ObjectId.GenerateNewStringId();
            Timestamp = DateTime.Now;
            Sequence = 1;
        }

        public abstract void SerializeTo(IDictionary<string, string> serializableInfo);
        public abstract void RestoreFrom(IDictionary<string, string> serializableInfo);

        /// <summary>Returns null by default.
        /// </summary>
        /// <returns></returns>
        public virtual string GetRoutingKey()
        {
            return null;
        }
        /// <summary>Returns the full type name of the current exception.
        /// </summary>
        /// <returns></returns>
        public virtual string GetTypeName()
        {
            return this.GetType().FullName;
        }
    }
}
