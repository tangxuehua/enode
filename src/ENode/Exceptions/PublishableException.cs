using System;
using ECommon.Utilities;

namespace ENode.Exceptions
{
    public class PublishableException : Exception
    {
        public string Id { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public PublishableException()
        {
            Id = ObjectId.GenerateNewStringId();
        }
    }
}
