using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Domain;
using ENode.Eventing;
using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandMessageHandler.
    /// </summary>
    public class DefaultCommandMessageHandler : MessageHandler<EventCommittingContext>, ICommandMessageHandler
    {
        #region Private Variables

        private readonly IWaitingCommandCache _waitingCommandCache;
        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IEventPublisher _committedEventSender;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ICommandContext _commandContext;
        private readonly ITrackingContext _trackingContext;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="waitingCommandCache"></param>
        /// <param name="processingCommandCache"></param>
        /// <param name="commandHandlerProvider"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="eventSender"></param>
        /// <param name="committedEventSender"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="commandContext"></param>
        /// <param name="loggerFactory"></param>
        /// <exception cref="Exception"></exception>
        public DefaultCommandMessageHandler(
            IWaitingCommandCache waitingCommandCache,
            IProcessingCommandCache processingCommandCache,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IEventPublisher committedEventSender,
            IActionExecutionService actionExecutionService,
            ICommandContext commandContext,
            ILoggerFactory loggerFactory)
        {
            _waitingCommandCache = waitingCommandCache;
            _processingCommandCache = processingCommandCache;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _committedEventSender = committedEventSender;
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

        /// <summary>Handle the given command message.
        /// </summary>
        /// <param name="message">The command message.</param>
        public override void Handle(Message<EventCommittingContext> message)
        {
            //TODO
            //if (!CheckWaitingCommand(message.Payload))
            //{
            //    HandleCommand(message.Payload);
            //}
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
        protected virtual void HandleCommand(ICommand command)
        {
            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);

            if (commandHandler == null)
            {
                var errorMessage = string.Format("Command handler not found for {0}", command.GetType().FullName);
                _logger.Fatal(errorMessage);
                return;
            }

            try
            {
                _processingCommandCache.Add(command);
                commandHandler.Handle(_commandContext, command);
                var dirtyAggregate = GetDirtyAggregate(_trackingContext);
                if (dirtyAggregate != null)
                {
                    CommitAggregate(dirtyAggregate, command);
                }
                else
                {
                    _logger.Info("No dirty aggregate found, finish the command execution directly.");
                }
            }
            catch (Exception ex)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("Exception raised when {0} handling {1}, command id:{2}.", commandHandlerType.Name, command.GetType().Name, command.Id);
                _logger.Error(errorMessage, ex);
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
        private void CommitAggregate(IAggregateRoot dirtyAggregate, ICommand command)
        {
            var eventStream = CreateEventStream(dirtyAggregate, command);

            if (eventStream.Events.Any(x => x is ISourcingEvent))
            {
                _actionExecutionService.TryAction(
                    "SendEvents",
                    () => SendEvents(eventStream),
                    3,
                    null);
            }
            else
            {
                _actionExecutionService.TryAction(
                    "PublishEvents",
                    () => PublishEvents(eventStream),
                    3,
                    new ActionInfo("PublishEventsCallback", data => { _processingCommandCache.TryRemove(eventStream.CommandId); return true; }, null, null));
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
                //_uncommittedEventSender.Send(eventStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when sending events:{0}", eventStream), ex);
                return false;
            }
        }
        private bool PublishEvents(EventStream eventStream)
        {
            try
            {
                //_committedEventSender.Send(eventStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream), ex);
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