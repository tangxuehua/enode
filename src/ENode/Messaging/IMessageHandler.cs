using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message handler.
    /// </summary>
    public interface IMessageHandler { }
    /// <summary>Represents a message handler.
    /// </summary>
    public interface IMessageHandler<in T> : IMessageHandler where T : class, IMessage
    {
        /// <summary>Handle the given message async.
        /// </summary>
        /// <param name="message"></param>
        Task<AsyncTaskResult> HandleAsync(T message);
    }
    /// <summary>Represents a message handler.
    /// </summary>
    public interface IMessageHandler<in T1, in T2> : IMessageHandler
        where T1 : class, IMessage
        where T2 : class, IMessage
    {
        /// <summary>Handle the given messages async.
        /// </summary>
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        Task<AsyncTaskResult> HandleAsync(T1 message1, T2 message2);
    }
    /// <summary>Represents a message handler.
    /// </summary>
    public interface IMessageHandler<in T1, in T2, in T3> : IMessageHandler
        where T1 : class, IMessage
        where T2 : class, IMessage
        where T3 : class, IMessage
    {
        /// <summary>Handle the given messages async.
        /// </summary>
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        Task<AsyncTaskResult> HandleAsync(T1 message1, T2 message2, T3 message3);
    }
}
