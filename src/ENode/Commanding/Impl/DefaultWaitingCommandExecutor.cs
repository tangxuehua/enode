using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Retring;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of waiting command executor interface.
    /// </summary>
    public class DefaultWaitingCommandExecutor : DefaultCommandExecutor, IWaitingCommandExecutor
    {
        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="waitingCommandCache"></param>
        /// <param name="processingCommandCache"></param>
        /// <param name="commandHandlerProvider"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="eventSender"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="commandContext"></param>
        /// <param name="loggerFactory"></param>
        public DefaultWaitingCommandExecutor(
            IWaitingCommandCache waitingCommandCache,
            IProcessingCommandCache processingCommandCache,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IEventSender eventSender,
            IEventPublisher eventPublisher,
            IActionExecutionService actionExecutionService,
            ICommandContext commandContext,
            ILoggerFactory loggerFactory)
            : base(waitingCommandCache,
                processingCommandCache,
                commandHandlerProvider,
                aggregateRootTypeProvider,
                eventSender,
                eventPublisher,
                actionExecutionService,
                commandContext,
                loggerFactory)
        {
        }

        #endregion

        /// <summary>Check whether the command is added to the waiting command cache.
        /// Override the base method, and always returns false.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override bool CheckWaitingCommand(ICommand command)
        {
            return false;
        }
    }
}