using System;
using System.Linq;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
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

        private readonly IProcessingCommandCache _processingCommandCache;
        private readonly ICommandAsyncResultManager _commandAsyncResultManager;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IEventSender _eventSender;
        private readonly IRetryService _retryService;
        private readonly ICommandContext _commandContext;
        private readonly ITrackingContext _trackingContext;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="processingCommandCache"></param>
        /// <param name="commandAsyncResultManager"></param>
        /// <param name="commandHandlerProvider"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="eventSender"></param>
        /// <param name="retryService"></param>
        /// <param name="commandContext"></param>
        /// <param name="loggerFactory"></param>
        /// <exception cref="Exception"></exception>
        public DefaultCommandExecutor(
            IProcessingCommandCache processingCommandCache,
            ICommandAsyncResultManager commandAsyncResultManager,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IEventSender eventSender,
            IRetryService retryService,
            ICommandContext commandContext,
            ILoggerFactory loggerFactory)
        {
            _processingCommandCache = processingCommandCache;
            _commandAsyncResultManager = commandAsyncResultManager;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _eventSender = eventSender;
            _retryService = retryService;
            _commandContext = commandContext;
            _trackingContext = commandContext as ITrackingContext;
            _logger = loggerFactory.Create(GetType().Name);

            if (_trackingContext == null)
            {
                throw new Exception("Command context must also implement ITrackingContext interface.");
            }
        }

        #endregion

        /// <summary>Execute the given command message.
        /// </summary>
        /// <param name="message">The command message.</param>
        /// <param name="queue">The queue which the command message belongs to.</param>
        public override void Execute(ICommand message, IMessageQueue<ICommand> queue)
        {
            var command = message;
            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);

            if (commandHandler == null)
            {
                var errorMessage = string.Format("Command handler not found for {0}", command.GetType().FullName);
                _logger.Fatal(errorMessage);
                _commandAsyncResultManager.TryComplete(command.Id, null, new ErrorInfo(errorMessage));
                FinishExecution(command, queue);
                return;
            }

            try
            {
                _trackingContext.Clear();
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
                    _commandAsyncResultManager.TryComplete(command.Id, null);
                    FinishExecution(command, queue);
                }
            }
            catch (Exception ex)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("Exception raised when {0} handling {1}, command id:{2}.", commandHandlerType.Name, command.GetType().Name, command.Id);
                _logger.Error(errorMessage, ex);
                _commandAsyncResultManager.TryComplete(command.Id, null, new ErrorInfo(errorMessage, ex));
                FinishExecution(command, queue);
            }
            finally
            {
                _trackingContext.Clear();
            }
        }

        #region Private Methods

        private static AggregateRoot GetDirtyAggregate(ITrackingContext trackingContext)
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
        private EventStream BuildEvents(AggregateRoot aggregateRoot, ICommand command)
        {
            var uncommittedEvents = aggregateRoot.GetUncommittedEvents().ToList();
            var aggregateRootType = aggregateRoot.GetType();
            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRootType);

            return new EventStream(
                aggregateRoot.UniqueId,
                aggregateRootName,
                aggregateRoot.Version + 1,
                command.Id,
                DateTime.UtcNow,
                uncommittedEvents);
        }
        private void CommitAggregate(AggregateRoot dirtyAggregate, ICommand command, IMessageQueue<ICommand> queue)
        {
            var eventStream = BuildEvents(dirtyAggregate, command);
            _retryService.TryAction("TrySendEvent", () => TrySendEvent(eventStream), 3, () => FinishExecution(command, queue));
        }
        private bool TrySendEvent(EventStream eventStream)
        {
            try
            {
                _eventSender.Send(eventStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when tring to send events, events info:{0}.", eventStream.GetStreamInformation()), ex);
                return false;
            }
        }

        #endregion
    }
}