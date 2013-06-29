using System;
using System.Linq;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    public class DefaultCommandExecutor : ICommandExecutor
    {
        #region Private Variables

        private IProcessingCommandCache _processingCommandCache;
        private ICommandAsyncResultManager _commandAsyncResultManager;
        private ICommandHandlerProvider _commandHandlerProvider;
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private IMemoryCacheRefreshService _memoryCacheRefreshService;
        private IRetryCommandService _retryCommandService;
        private IEventStore _eventStore;
        private IEventPublisher _eventPublisher;
        private ICommandContext _commandContext;
        private ITrackingContext _trackingContext;
        private ILogger _logger;

        #endregion

        #region Constructors

        public DefaultCommandExecutor(
            IProcessingCommandCache processingCommandCache,
            ICommandAsyncResultManager commandAsyncResultManager,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IMemoryCacheRefreshService memoryCacheRefreshService,
            IRetryCommandService retryCommandService,
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            ICommandContext commandContext,
            ILoggerFactory loggerFactory)
        {
            _processingCommandCache = processingCommandCache;
            _commandAsyncResultManager = commandAsyncResultManager;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _memoryCacheRefreshService = memoryCacheRefreshService;
            _retryCommandService = retryCommandService;
            _eventStore = eventStore;
            _eventPublisher = eventPublisher;
            _commandContext = commandContext;
            _trackingContext = commandContext as ITrackingContext;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        public bool Execute(ICommand command)
        {
            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);

            if (commandHandler == null)
            {
                _logger.ErrorFormat("Command handler not found for {0}", command.GetType().FullName);
                return true;
            }

            try
            {
                _trackingContext.Clear();
                _processingCommandCache.Add(command);
                commandHandler.Handle(_commandContext, command);
                var eventStream = BuildEventStream(_trackingContext, command);
                if (eventStream != null)
                {
                    return SubmitChanges(eventStream);
                }
            }
            catch (Exception ex)
            {
                var commandHandlerType = (commandHandler as ICommandHandlerWrapper).GetInnerCommandHandler().GetType();
                _logger.Error(string.Format("Unknown exception raised when {0} handling {1}, command id:{2}.", commandHandlerType.Name, command.GetType().Name, command.Id), ex);
                _commandAsyncResultManager.TryComplete(command.Id, ex);
            }
            finally
            {
                _trackingContext.Clear();
                _processingCommandCache.TryRemove(command.Id);
            }

            return true;
        }
        public void Execute(ICommandContext context, ICommand command)
        {
            var trackingContext = context as ITrackingContext;
            if (trackingContext == null)
            {
                throw new Exception("Command context must also implement ITrackingContext interface.");
            }

            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);
            if (commandHandler == null)
            {
                throw new Exception(string.Format("Command handler not found for {0}", command.GetType().FullName));
            }

            commandHandler.Handle(context, command);

            var eventStream = BuildEventStream(trackingContext, command);
            if (eventStream != null)
            {
                //Persist event stream.
                _eventStore.Append(eventStream);

                //Refresh memory cache.
                try { _memoryCacheRefreshService.Refresh(eventStream); }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Unknown exception raised when refreshing memory cache for event stream:{0}", eventStream.GetStreamInformation()), ex);
                }

                //Publish event stream.
                try { _eventPublisher.Publish(eventStream); }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Unknown exception raised when publishing event stream:{0}", eventStream.GetStreamInformation()), ex);
                }
            }
        }

        private EventStream BuildEventStream(ITrackingContext trackingContext, ICommand command)
        {
            var trackedAggregateRoots = trackingContext.GetTrackedAggregateRoots();
            var dirtyAggregateRoots = trackedAggregateRoots.Where(x => x.GetUncommittedEvents().Count() > 0);
            var dirtyAggregateRootCount = dirtyAggregateRoots.Count();

            if (dirtyAggregateRootCount == 0)
            {
                return null;
            }
            else if (dirtyAggregateRootCount > 1)
            {
                throw new Exception("Detected more than one new or modified aggregates.");
            }

            var dirtyAggregateRoot = dirtyAggregateRoots.Single();
            var uncommittedEvents = dirtyAggregateRoot.GetUncommittedEvents();
            var aggregateRootType = dirtyAggregateRoot.GetType();
            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRootType);

            return new EventStream(
                dirtyAggregateRoot.UniqueId,
                aggregateRootName,
                dirtyAggregateRoot.Version + 1,
                command.Id,
                DateTime.UtcNow,
                uncommittedEvents);
        }
        private bool SubmitChanges(EventStream stream)
        {
            bool executed = false;

            //Persist event stream.
            var result = PersistEventStream(stream);

            if (result == EventStreamPersistResult.Success)
            {
                //Refresh memory cache.
                try { _memoryCacheRefreshService.Refresh(stream); }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Unknown exception raised when refreshing memory cache for event stream:{0}", stream.GetStreamInformation()), ex);
                }

                //Publish event stream.
                try { _eventPublisher.Publish(stream); executed = true; }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Unknown exception raised when publishing event stream:{0}", stream.GetStreamInformation()), ex);
                }

                //Complete command async result if exist.
                _commandAsyncResultManager.TryComplete(stream.CommandId, null);
            }
            else if (result == EventStreamPersistResult.Retried)
            {
                executed = true;
            }

            return executed;
        }
        private EventStreamPersistResult PersistEventStream(EventStream stream)
        {
            try
            {
                _eventStore.Append(stream);
                return EventStreamPersistResult.Success;
            }
            catch (Exception ex)
            {
                return ProcessException(ex, stream);
            }
        }
        private EventStreamPersistResult ProcessException(Exception exception, EventStream stream)
        {
            if (exception is ConcurrentException)
            {
                if (IsEventStreamCommitted(stream))
                {
                    return EventStreamPersistResult.Success;
                }

                var commandInfo = _processingCommandCache.Get(stream.CommandId);

                _logger.Error(string.Format(
                    "Concurrent exception raised when persisting event stream, command:{0}, event stream info:{1}",
                    commandInfo.Command.GetType().Name,
                    stream.GetStreamInformation()), exception);

                //Enforce to refresh memory cache before retring the command
                //to enusre that when we retring the command, the memeory cache is at the latest status.
                _memoryCacheRefreshService.Refresh(_aggregateRootTypeProvider.GetAggregateRootType(stream.AggregateRootName), stream.AggregateRootId);

                _retryCommandService.RetryCommand(commandInfo, exception);

                return EventStreamPersistResult.Retried;
            }
            else
            {
                var commandInfo = _processingCommandCache.Get(stream.CommandId);
                _logger.Error(string.Format(
                    "Unknown exception raised when persisting event stream, command:{0}, event stream info:{1}",
                    commandInfo.Command.GetType().Name,
                    stream.GetStreamInformation()), exception);
                return EventStreamPersistResult.UnknownException;
            }
        }
        private bool IsEventStreamCommitted(EventStream stream)
        {
            return _eventStore.IsEventStreamExist(stream.AggregateRootId, _aggregateRootTypeProvider.GetAggregateRootType(stream.AggregateRootName), stream.Id);
        }

        enum EventStreamPersistResult
        {
            Success,
            Retried,
            UnknownException
        }
    }
}
