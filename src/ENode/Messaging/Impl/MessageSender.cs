using System;

namespace ENode.Messaging.Impl
{
    /// <summary>The abstract implementation of IMessageSender.
    /// </summary>
    public abstract class MessageSender<TMessageQueueRouter, TMessageQueue, TMessagePayload> : IMessageSender<TMessagePayload>
        where TMessageQueue : class, IMessageQueue<TMessagePayload>
        where TMessageQueueRouter : class, IMessageQueueRouter<TMessageQueue, TMessagePayload>
    {
        private readonly TMessageQueueRouter _queueRouter;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="queueRouter"></param>
        public MessageSender(TMessageQueueRouter queueRouter)
        {
            _queueRouter = queueRouter;
        }

        /// <summary>Send the given payload object to a specific message queue.
        /// </summary>
        /// <param name="payload"></param>
        public void Send(TMessagePayload payload)
        {
            Send(payload, Guid.NewGuid());
        }
        /// <summary>Send the given payload object to a specific message queue.
        /// </summary>
        public void Send(TMessagePayload payload, Guid messageId)
        {
            Send(payload, messageId, null);
        }
        /// <summary>Send the given payload object to a specific message queue.
        /// </summary>
        public void Send(TMessagePayload payload, Guid messageId, Action<Guid, object> messageHandledCallback)
        {
            var queue = _queueRouter.Route(payload);
            if (queue == null)
            {
                throw new Exception("Could not route an appropriate message queue.");
            }

            var message = new Message<TMessagePayload>(messageId, payload, queue.Name);
            if (messageHandledCallback != null)
            {
                message.Handled += messageHandledCallback;
            }
            queue.Enqueue(message);
        }
    }
}
