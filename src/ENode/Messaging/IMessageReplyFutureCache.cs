using System;

namespace ENode.Messaging
{
    /// <summary>Represents a memory cache for caching all the message reply future objects.
    /// </summary>
    public interface IMessageReplyFutureCache<TReplyFuture>
    {
        /// <summary>Add a message reply future object into cache.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="replyFuture"></param>
        void Add(Guid messageId, TReplyFuture replyFuture);
        /// <summary>Try to remove a reply future object from cache.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="replyFuture"></param>
        bool TryRemove(Guid messageId, out TReplyFuture replyFuture);
    }
}
