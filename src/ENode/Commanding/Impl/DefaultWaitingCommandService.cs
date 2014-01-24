using System;
using ECommon.Logging;
using ENode.Configurations;
using ENode.Infrastructure.Logging;
using ENode.Messaging;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of IWaitingCommandService.
    /// </summary>
    public class DefaultWaitingCommandService : IWaitingCommandService
    {
        private readonly IWaitingCommandCache _waitingCommandCache;
        private ICommandQueue _waitingCommandQueue;

        /// <summary>Parameterized costructor.
        /// </summary>
        /// <param name="waitingCommandCache"></param>
        /// <param name="loggerFactory"></param>
        public DefaultWaitingCommandService(IWaitingCommandCache waitingCommandCache, ILoggerFactory loggerFactory)
        {
            _waitingCommandCache = waitingCommandCache;
        }

        /// <summary>Try to send an available waiting command to the waiting command queue.
        /// </summary>
        /// <param name="aggregateRootId">The aggregate root id.</param>
        public void SendWaitingCommand(object aggregateRootId)
        {
            if (_waitingCommandQueue == null)
            {
                _waitingCommandQueue = ENodeConfiguration.Instance.GetWaitingCommandQueue();
            }
            var command = _waitingCommandCache.FetchWaitingCommand(aggregateRootId);

            if (command != null)
            {
                _waitingCommandQueue.Enqueue(new Message<ICommand>(Guid.NewGuid(), command, _waitingCommandQueue.Name));
            }
        }
    }
}
