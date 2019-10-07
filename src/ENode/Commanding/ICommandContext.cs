using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using System.Threading.Tasks;

namespace ENode.Commanding
{
    /// <summary>Represents a command context for aggregate command handler handling command.
    /// </summary>
    public interface ICommandContext
    {
        /// <summary>Add a new aggregate into the current command context.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        void Add(IAggregateRoot aggregateRoot);
        /// <summary>Add a new aggregate into the current command context synchronously, and then return a completed task object.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        Task AddAsync(IAggregateRoot aggregateRoot);
        /// <summary>Get an aggregate from the current command context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="firstFromCache"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(object id, bool firstFromCache = true) where T : class, IAggregateRoot;
        /// <summary>Set the command handle result.
        /// </summary>
        /// <param name="result"></param>
        void SetResult(string result);
        /// <summary>Get the command handle result.
        /// </summary>
        /// <returns></returns>
        string GetResult();
        /// <summary>Set an application message.
        /// </summary>
        /// <param name="applicationMessage"></param>
        void SetApplicationMessage(IApplicationMessage applicationMessage);
        /// <summary>Get an application message.
        /// </summary>
        /// <returns></returns>
        IApplicationMessage GetApplicationMessage();
    }
}
