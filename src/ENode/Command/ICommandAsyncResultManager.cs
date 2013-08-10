using System;

namespace ENode.Commanding {
    public interface ICommandAsyncResultManager {
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
        void TryComplete(Guid commandId);
        /// <summary>Try to complete a command async result if exist.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="exception"></param>
        void TryComplete(Guid commandId, string errorMessage, Exception exception);
    }
}
