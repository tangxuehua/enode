using System;
using System.Collections.Concurrent;

namespace ENode.Commanding
{
    /// <summary>The default implementation of ICommandAsyncResultManager.
    /// </summary>
    public class DefaultCommandAsyncResultManager : ICommandAsyncResultManager
    {
        private readonly ConcurrentDictionary<Guid, CommandAsyncResult> _commandAsyncResultDict = new ConcurrentDictionary<Guid, CommandAsyncResult>();

        /// <summary>Add the command async result for a command.
        /// </summary>
        /// <param name="commandId">The commandId.</param>
        /// <param name="commandAsyncResult">The command async result.</param>
        public void Add(Guid commandId, CommandAsyncResult commandAsyncResult)
        {
            if (!_commandAsyncResultDict.TryAdd(commandId, commandAsyncResult))
            {
                throw new Exception(string.Format("Command with id '{0}' is already exist.", commandId));
            }
        }
        /// <summary>Remove the specified command async result for the given commandId.
        /// </summary>
        /// <param name="commandId">The commandId.</param>
        public void Remove(Guid commandId)
        {
            CommandAsyncResult commandAsyncResult;
            _commandAsyncResultDict.TryRemove(commandId, out commandAsyncResult);
        }
        /// <summary>Try to complete the command async result for the given commandId.
        /// </summary>
        /// <param name="commandId">The commandId.</param>
        /// <param name="aggregateRootId">The id of the aggregate which was created or updated by the command.</param>
        public void TryComplete(Guid commandId, string aggregateRootId)
        {
            TryComplete(commandId, aggregateRootId, null, null);
        }
        /// <summary>Try to complete the command async result for the given commandId.
        /// </summary>
        /// <param name="commandId">The commandId.</param>
        /// <param name="aggregateRootId">The id of the aggregate which was created or updated by the command.</param>
        /// <param name="errorMessage">The error message if the command execution has any error.</param>
        /// <param name="exception">The execption if the command execution has any error.</param>
        public void TryComplete(Guid commandId, string aggregateRootId, string errorMessage, Exception exception)
        {
            CommandAsyncResult commandAsyncResult;
            if (_commandAsyncResultDict.TryRemove(commandId, out commandAsyncResult))
            {
                commandAsyncResult.Complete(aggregateRootId, errorMessage, exception);
            }
        }
    }
}
