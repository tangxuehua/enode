using ENode.Infrastructure;

namespace ENode.Messaging
{
    /// <summary>Represents a message handler.
    /// </summary>
    public interface IMessageHandler : IProxyHandler
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message"></param>
        void Handle(object message);
    }
    /// <summary>Represents a message handler.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<in TMessage> where TMessage : class, IMessage
    {
        /// <summary>Handle the given message.
        /// </summary>
        /// <param name="message"></param>
        void Handle(TMessage message);
    }
}
