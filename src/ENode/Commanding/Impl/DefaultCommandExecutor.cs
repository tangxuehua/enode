using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Domain;
using ENode.Eventing;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandExecutor : ICommandExecutor
    {
        #region Private Variables

        private readonly IWaitingCommandCache _waitingCommandCache;
        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly ICommitEventService _commitEventService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="waitingCommandCache"></param>
        /// <param name="processingCommandCache"></param>
        /// <param name="commandHandlerProvider"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="commitEventService"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommandExecutor(
            IWaitingCommandCache waitingCommandCache,
            IProcessingCommandCache processingCommandCache,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            ICommitEventService commitEventService,
            IEventPublisher eventPublisher,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _waitingCommandCache = waitingCommandCache;
            _processingCommandCache = processingCommandCache;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _commitEventService = commitEventService;
            _eventPublisher = eventPublisher;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        #region Public Methods

        /// <summary>Executes the given command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The context when executing the command.</param>
        public void Execute(ICommand command, ICommandExecuteContext context)
        {
            if (!CheckWaitingCommand(command))
            {
                HandleCommand(command, context);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>Check whether the command is added to the waiting command cache.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected virtual bool CheckWaitingCommand(ICommand command)
        {
            if (command is ICreatingAggregateCommand)
            {
                return false;
            }
            return _waitingCommandCache.AddWaitingCommand(command.AggregateRootId, command);
        }
        /// <summary>Acutally handle the command.
        /// </summary>
        /// <param name="command"></param>
        protected virtual void HandleCommand(ICommand command, ICommandExecuteContext context)
        {
            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);
            if (commandHandler == null)
            {
                _logger.Fatal(string.Format("Command handler not found for {0}", command.GetType().FullName));
                context.OnCommandExecuted(command);
                return;
            }

            try
            {
                _processingCommandCache.Add(command);
                commandHandler.Handle(context, command);
                CommitChanges(context, command);
            }
            catch (Exception ex)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}], commandId:{2}, aggregateRootId:{3}.", commandHandlerType.Name, command.GetType().Name, command.Id, command.AggregateRootId), ex);
                throw;
            }
        }

        #endregion

        #region Private Methods

        private static IAggregateRoot GetDirtyAggregate(ITrackingContext trackingContext)
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
                throw new Exception("Detected more than one dirty aggregates.");
            }

            return dirtyAggregateRoots.Single();
        }
        private void CommitChanges(ICommandExecuteContext commandExecuteContext, ICommand command)
        {
            var dirtyAggregate = GetDirtyAggregate(commandExecuteContext);
            if (dirtyAggregate == null)
            {
                _logger.Info("No dirty aggregate found.");
                return;
            }
            var eventStream = CreateEventStream(dirtyAggregate, command);
            if (eventStream.Events.Any(x => x is ISourcingEvent))
            {
                _commitEventService.CommitEvent(new EventCommittingContext(eventStream, command, commandExecuteContext));
            }
            else
            {
                PublishEvents(eventStream, command, commandExecuteContext);
            }
        }
        private EventStream CreateEventStream(IAggregateRoot aggregateRoot, ICommand command)
        {
            var uncommittedEvents = aggregateRoot.GetUncommittedEvents().ToList();
            ValidateEvents(aggregateRoot, uncommittedEvents);

            var aggregateRootType = aggregateRoot.GetType();
            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRootType);
            var aggregateRootId = uncommittedEvents.First().AggregateRootId;

            return new EventStream(
                aggregateRootId,
                aggregateRootName,
                aggregateRoot.Version + 1,
                command.Id,
                DateTime.UtcNow,
                uncommittedEvents);
        }
        private void PublishEvents(EventStream eventStream, ICommand command, ICommandExecuteContext commandExecuteContext)
        {
            var publishEvents = new Func<bool>(() =>
            {
                try
                {
                    _eventPublisher.Send(eventStream);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream.GetStreamInformation()), ex);
                    return false;
                }
            });

            _actionExecutionService.TryAction("PublishEvents", publishEvents, 3, new ActionInfo("PublishEventsCallback", data =>
            {
                _processingCommandCache.TryRemove(eventStream.CommandId);
                commandExecuteContext.OnCommandExecuted(command);
                return true;
            }, null, null));
        }
        private void ValidateEvents(IAggregateRoot aggregateRoot, IList<IDomainEvent> evnts)
        {
            var aggregateRootId = evnts[0].AggregateRootId;
            for (var index = 1; index < evnts.Count; index++)
            {
                if (!object.Equals(evnts[index].AggregateRootId, aggregateRootId))
                {
                    throw new Exception(string.Format("Wrong aggregate root id of domain event: {0}.", evnts[index].GetType().FullName));
                }
            }
            if (aggregateRoot.Version > 0)
            {
                if (!object.Equals(aggregateRoot.UniqueId, aggregateRootId))
                {
                    throw new Exception(string.Format("Mismatch aggregate root id. Expected:{0}, Actual:{1}", aggregateRoot.UniqueId, aggregateRootId));
                }
            }
        }

        #endregion
    }
}
