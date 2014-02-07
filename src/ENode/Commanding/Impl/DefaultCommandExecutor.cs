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
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly ICommitEventService _commitEventService;
        private readonly IPublishEventService _publishEventService;
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
        /// <param name="publishEventService"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommandExecutor(
            IWaitingCommandCache waitingCommandCache,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            ICommitEventService commitEventService,
            IPublishEventService publishEventService,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _waitingCommandCache = waitingCommandCache;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _commitEventService = commitEventService;
            _publishEventService = publishEventService;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().Name);
            _commitEventService.SetCommandExecutor(this);
        }

        #endregion

        #region Public Methods

        /// <summary>Executes the given command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The context when executing the command.</param>
        public void Execute(ICommand command, ICommandExecuteContext context)
        {
            HandleCommand(new ProcessingCommand(command, context));
        }

        #endregion

        #region Private Methods

        private void HandleCommand(ProcessingCommand processingCommand)
        {
            if (processingCommand.CommandExecuteContext.CheckCommandWaiting && TryToAddWaitingCommand(processingCommand))
            {
                _logger.DebugFormat("Added a waiting command:[Type={0},Id={1},AggregateRootId={2}]", processingCommand.Command.GetType().Name, processingCommand.Command.Id, processingCommand.Command.AggregateRootId);
                return;
            }

            var command = processingCommand.Command;
            var context = processingCommand.CommandExecuteContext;
            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);
            if (commandHandler == null)
            {
                var errorMessage = string.Format("Command handler not found for [{0}].", command.GetType().FullName);
                _logger.Error(errorMessage);
                context.OnCommandExecuted(new CommandResult(command, errorMessage));
                return;
            }

            try
            {
                commandHandler.Handle(context, command);
                CommitChanges(processingCommand);
            }
            catch (Exception ex)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("Exception raised when [{0}] handling [{1}], commandId:{2}, aggregateRootId:{3}.", commandHandlerType.Name, command.GetType().Name, command.Id, command.AggregateRootId);
                _logger.Error(errorMessage, ex);
                context.OnCommandExecuted(new CommandResult(command, errorMessage));
            }
        }
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
                _logger.WarnFormat("No aggregate created or modified by [{0}], commandId:{1}, aggregateRootId:{2}.", command.GetType().Name, command.Id, command.AggregateRootId);
                context.OnCommandExecuted(new CommandResult(command));
                return;
            }
            var eventStream = CreateEventStream(dirtyAggregate, command);
            if (eventStream.Events.Any(x => x is ISourcingEvent))
            {
                _commitEventService.CommitEvent(new EventProcessingContext(dirtyAggregate, eventStream, processingCommand));
            }
            else
            {
                _publishEventService.PublishEvent(new EventProcessingContext(dirtyAggregate, eventStream, processingCommand));
            }
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

            var aggregateRootType = aggregateRoot.GetType();
            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRootType);
            var aggregateRootId = uncommittedEvents.First().AggregateRootId;

            return new EventStream(
                command.Id,
                aggregateRootId,
                aggregateRootName,
                aggregateRoot.Version + 1,
                DateTime.Now,
                uncommittedEvents);
        }

        #endregion
    }
}
