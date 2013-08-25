using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>An interface of manager to manage the command async result.
    /// </summary>
    public interface ICommandAsyncResultManager
    {
        /// <summary>Add a command async result.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="commandAsyncResult"></param>
        void Add(Guid commandId, CommandAsyncResult commandAsyncResult);
        /// <summary>Remove a command async result.
        /// </summary>
        /// <param name="commandId"></param>
        void Remove(Guid commandId);
        /// <summary>Try to complete a command async result if exist;
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="aggregateRootId"></param>
        void TryComplete(Guid commandId, string aggregateRootId);
        /// <summary>Try to complete a command async result if it exist.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="errorInfo"></param>
        void TryComplete(Guid commandId, string aggregateRootId, ErrorInfo errorInfo);
    }
}
