using System;

namespace ENode.Commanding
{
    public interface ICommandAsyncResultManager
    {
        /// <summary>Add a command async result.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="commandAsyncResult"></param>
        void Add(Guid commandId, CommandAsyncResult commandAsyncResult);
        /// <summary>Complete a command async result if exists.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="exception"></param>
        void TryComplete(Guid commandId, Exception exception);
    }
}
