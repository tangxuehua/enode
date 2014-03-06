using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandExecutor : ICommandExecutor
    {
        #region Private Variables

        private readonly IWaitingCommandCache _waitingCommandCache;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly IAggregateRootTypeCodeProvider _aggregateRootTypeProvider;
        private readonly ICommitEventService _commitEventService;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="waitingCommandCache"></param>
        /// <param name="commandHandlerProvider"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="commitEventService"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommandExecutor(
            IWaitingCommandCache waitingCommandCache,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeCodeProvider aggregateRootTypeProvider,
            ICommitEventService commitEventService,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _waitingCommandCache = waitingCommandCache;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _commitEventService = commitEventService;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().Name);
            _commitEventService.SetCommandExecutor(this);
        }

        #endregion

        #region Public Methods

        public void Execute(ProcessingCommand processingCommand)
        {
            if (processingCommand.CommandExecuteContext.CheckCommandWaiting && TryToAddWaitingCommand(processingCommand))
            {
                _logger.DebugFormat("Queued a waiting command:[commandType={0},commandI={1},aggregateRootId={2}]", processingCommand.Command.GetType().Name, processingCommand.Command.Id, processingCommand.Command.AggregateRootId);
                return;
            }

            var command = processingCommand.Command;
            var context = processingCommand.CommandExecuteContext;
            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);
            if (commandHandler == null)
            {
                var errorMessage = string.Format("Command handler not found for [{0}].", command.GetType().FullName);
                _logger.Error(errorMessage);
                context.OnCommandExecuted(command, CommandStatus.Failed, 0, errorMessage);
                return;
            }

            try
            {
                commandHandler.Handle(context, command);
                CommitChanges(processingCommand);
            }
            catch (DomainException domainException)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("{0} raised when [{1}] handling [{2}], commandId:{3}, aggregateRootId:{4}, exceptionMessage:{5}.",
                    domainException.GetType().Name,
                    commandHandlerType.Name,
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId,
                    domainException.Message);
                _logger.Error(errorMessage, domainException);
                context.OnCommandExecuted(command, CommandStatus.Failed, domainException.Code, domainException.Message);
            }
            catch (Exception ex)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("{0} raised when [{1}] handling [{2}], commandId:{3}, aggregateRootId:{4}, exceptionMessage:{5}.",
                    ex.GetType().Name,
                    commandHandlerType.Name,
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId,
                    ex.Message);
                _logger.Error(errorMessage, ex);
                context.OnCommandExecuted(command, CommandStatus.Failed, 0, ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private bool TryToAddWaitingCommand(ProcessingCommand processingCommand)
        {
            if (processingCommand.Command is ICreatingAggregateCommand)
            {
                return false;
            }
            return _waitingCommandCache.AddWaitingCommand(processingCommand.Command.AggregateRootId, processingCommand);
        }
        private void CommitChanges(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;
            var context = processingCommand.CommandExecuteContext;
            var dirtyAggregate = GetDirtyAggregate(context);
            if (dirtyAggregate == null)
            {
                _logger.DebugFormat("No aggregate created or modified by {0}[commandId={1},aggregateRootId={2}].",
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId);
                context.OnCommandExecuted(command, CommandStatus.NothingChanged, 0, null);
                return;
            }
            var eventStream = CreateEventStream(dirtyAggregate, command);
            _commitEventService.CommitEvent(new EventProcessingContext(dirtyAggregate, eventStream, processingCommand));
        }
        private IAggregateRoot GetDirtyAggregate(ITrackingContext trackingContext)
        {
            var trackedAggregateRoots = trackingContext.GetTrackedAggregateRoots();
            var dirtyAggregateRoots = trackedAggregateRoots.Where(x => x.GetUncommittedEvents().Any()).ToList();
            var dirtyAggregateRootCount = dirtyAggregateRoots.Count();

            if (dirtyAggregateRootCount == 0)
            {
                return null;
            }
            if (dirtyAggregateRootCount > 1)
            {
                throw new ENodeException("Detected more than one dirty aggregates [{0}].", dirtyAggregateRootCount);
            }

            return dirtyAggregateRoots.Single();
        }
        private EventStream CreateEventStream(IAggregateRoot aggregateRoot, ICommand command)
        {
            var uncommittedEvents = aggregateRoot.GetUncommittedEvents().ToList();
            aggregateRoot.ClearUncommittedEvents();

            var aggregateRootTypeCode = _aggregateRootTypeProvider.GetTypeCode(aggregateRoot.GetType());

            foreach (var evnt in uncommittedEvents)
            {
                evnt.Version = aggregateRoot.Version + 1;
            }
            return new EventStream(
                command.Id,
                aggregateRoot.UniqueId,
                aggregateRootTypeCode,
                aggregateRoot.Version + 1,
                DateTime.Now,
                uncommittedEvents);
        }

        #endregion
    }
}
