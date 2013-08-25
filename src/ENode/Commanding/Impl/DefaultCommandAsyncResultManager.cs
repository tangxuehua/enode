using System;
using System.Collections.Concurrent;
using ENode.Infrastructure;
using ENode.Infrastructure.Retring;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandAsyncResultManager.
    /// </summary>
    public class DefaultCommandAsyncResultManager : ICommandAsyncResultManager
    {
        private readonly IRetryService _retryService;
        private readonly ConcurrentDictionary<Guid, CommandAsyncResult> _commandAsyncResultDict = new ConcurrentDictionary<Guid, CommandAsyncResult>();

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="retryService"></param>
        public DefaultCommandAsyncResultManager(IRetryService retryService)
        {
            _retryService = retryService;
        }

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
            TryComplete(commandId, aggregateRootId, null);
        }
        /// <summary>Try to complete the command async result for the given commandId.
        /// </summary>
        /// <param name="commandId">The commandId.</param>
        /// <param name="aggregateRootId">The id of the aggregate which was created or updated by the command.</param>
        /// <param name="errorInfo">The error info if the command execution has any error.</param>
        public void TryComplete(Guid commandId, string aggregateRootId, ErrorInfo errorInfo)
        {
            CommandAsyncResult commandAsyncResult;
            if (!_commandAsyncResultDict.TryRemove(commandId, out commandAsyncResult))
            {
                return;
            }
            _retryService.TryAction("TryCompleteCommandAsyncResult", () => commandAsyncResult.Complete(aggregateRootId, errorInfo), 3, null);
        }
    }
}
