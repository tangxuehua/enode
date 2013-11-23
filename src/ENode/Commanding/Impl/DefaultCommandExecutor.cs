using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Retring;
using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of command executor interface.
    /// </summary>
    public class DefaultCommandExecutor : MessageExecutor<ICommand>, ICommandExecutor
    {
        #region Private Variables

        private readonly ICommandTaskManager _commandTaskManager;
        private readonly IWaitingCommandCache _waitingCommandCache;
        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IEventSender _eventSender;
        private readonly IEventPublisher _eventPublisher;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ICommandContext _commandContext;
        private readonly ITrackingContext _trackingContext;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandTaskManager"></param>
        /// <param name="waitingCommandCache"></param>
        /// <param name="processingCommandCache"></param>
        /// <param name="commandHandlerProvider"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="eventSender"></param>
        /// <param name="eventPublisher"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="commandContext"></param>
        /// <param name="loggerFactory"></param>
        /// <exception cref="Exception"></exception>
        public DefaultCommandExecutor(
            ICommandTaskManager commandTaskManager,
            IWaitingCommandCache waitingCommandCache,
            IProcessingCommandCache processingCommandCache,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IEventSender eventSender,
            IEventPublisher eventPublisher,
            IActionExecutionService actionExecutionService,
            ICommandContext commandContext,
            ILoggerFactory loggerFactory)
        {
            _commandTaskManager = commandTaskManager;
            _waitingCommandCache = waitingCommandCache;
            _processingCommandCache = processingCommandCache;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _eventSender = eventSender;
            _eventPublisher = eventPublisher;
            _actionExecutionService = actionExecutionService;
            _commandContext = commandContext;
            _trackingContext = commandContext as ITrackingContext;
            _logger = loggerFactory.Create(GetType().Name);

            if (_trackingContext == null)
            {
                throw new Exception("Command context must also implement ITrackingContext interface.");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Execute the given command message.
        /// </summary>
        /// <param name="message">The command message.</param>
        /// <param name="queue">The queue which the command message belongs to.</param>
        public override void Execute(ICommand command, IMessageQueue<ICommand> queue)
        {
            if (!CheckWaitingCommand(command))
            {
                HandleCommand(command, queue);
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
        /// <param name="queue"></param>
        protected virtual void HandleCommand(ICommand command, IMessageQueue<ICommand> queue)
        {
            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);

            if (commandHandler == null)
            {
                var errorMessage = string.Format("Command handler not found for {0}", command.GetType().FullName);
                _logger.Fatal(errorMessage);
                _commandTaskManager.CompleteCommandTask(command.Id, errorMessage);
                queue.Delete(command);
                return;
            }

            try
            {
                _processingCommandCache.Add(command);
                commandHandler.Handle(_commandContext, command);
                var dirtyAggregate = GetDirtyAggregate(_trackingContext);
                if (dirtyAggregate != null)
                {
                    CommitAggregate(dirtyAggregate, command, queue);
                }
                else
                {
                    _logger.Info("No dirty aggregate found, finish the command execution directly.");
                    queue.Delete(command);
                }
            }
            catch (Exception ex)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("Exception raised when {0} handling {1}, command id:{2}.", commandHandlerType.Name, command.GetType().Name, command.Id);
                _logger.Error(errorMessage, ex);
                _commandTaskManager.CompleteCommandTask(command.Id, ex);
                queue.Delete(command);
            }
            finally
            {
                _trackingContext.Clear();
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
        private void CommitAggregate(IAggregateRoot dirtyAggregate, ICommand command, IMessageQueue<ICommand> queue)
        {
            var eventStream = CreateEventStream(dirtyAggregate, command);

            if (eventStream.Events.Any(x => x is ISourcingEvent))
            {
                _actionExecutionService.TryAction(
                    "SendEvents",
                    () => SendEvents(eventStream),
                    3,
                    new ActionInfo("SendEventsCallback", data => { queue.Delete(command); return true; }, null, null));
            }
            else
            {
                _actionExecutionService.TryAction(
                    "PublishEvents",
                    () => PublishEvents(eventStream),
                    3,
                    new ActionInfo("PublishEventsCallback", data => { _processingCommandCache.TryRemove(eventStream.CommandId); queue.Delete(command); return true; }, null, null));
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
        private bool SendEvents(EventStream eventStream)
        {
            try
            {
                _eventSender.Send(eventStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when sending events:{0}", eventStream.GetStreamInformation()), ex);
                return false;
            }
        }
        private bool PublishEvents(EventStream eventStream)
        {
            try
            {
                _eventPublisher.Publish(eventStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream.GetStreamInformation()), ex);
                return false;
            }
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