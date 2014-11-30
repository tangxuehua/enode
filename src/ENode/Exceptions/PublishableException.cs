using System;
using ECommon.Utilities;

namespace ENode.Exceptions
{
    public class PublishableException : Exception
    {
        public string UniqueId { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public PublishableException()
        {
            UniqueId = ObjectId.GenerateNewStringId();
        }
    }
}
