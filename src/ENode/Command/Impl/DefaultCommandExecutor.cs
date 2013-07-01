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
        private IMemoryCache _memoryCache;
        private IRepository _repository;
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
            IMemoryCache memoryCache,
            IRepository repository,
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
            _memoryCache = memoryCache;
            _repository = repository;
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
                _logger.FatalFormat("Command handler not found for {0}", command.GetType().FullName);
                return true;
            }

            try
            {
                _trackingContext.Clear();
                _processingCommandCache.Add(command);
                commandHandler.Handle(_commandContext, command);
                var dirtyAggregate = GetDirtyAggregate(_trackingContext);
                if (dirtyAggregate != null)
                {
                    var eventStream = BuildEventStream(dirtyAggregate, command);
                    if (eventStream != null)
                    {
                        return SubmitChanges(dirtyAggregate, eventStream, command);
                    }
                }
            }
            catch (Exception ex)
            {
                var commandHandlerType = (commandHandler as ICommandHandlerWrapper).GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("Unknown exception raised when {0} handling {1}, command id:{2}.", commandHandlerType.Name, command.GetType().Name, command.Id);
                _logger.Error(errorMessage, ex);
                _commandAsyncResultManager.TryComplete(command.Id, errorMessage, ex);
            }
            finally
            {
                _trackingContext.Clear();
                _processingCommandCache.TryRemove(command.Id);
            }

            return true;
        }

        private AggregateRoot GetDirtyAggregate(ITrackingContext trackingContext)
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

            return dirtyAggregateRoots.Single();
        }
        private EventStream BuildEventStream(AggregateRoot aggregateRoot, ICommand command)
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
        private bool SubmitChanges(AggregateRoot aggregateRoot, EventStream eventStream, ICommand command)
        {
            bool executed = false;

            //Persist event stream.
            EventStreamPersistResult result;

            try
            {
                _eventStore.Append(eventStream);
                result = EventStreamPersistResult.Success;
            }
            catch (Exception ex)
            {
                result = ProcessException(ex, eventStream);
            }

            if (result == EventStreamPersistResult.Success)
            {
                //Refresh memory cache.
                try { RefreshMemoryCache(aggregateRoot, eventStream); }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Unknown exception raised when refreshing memory cache for event stream:{0}", eventStream.GetStreamInformation()), ex);
                }

                //Publish event stream.
                try { _eventPublisher.Publish(eventStream); executed = true; }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Unknown exception raised when publishing event stream:{0}", eventStream.GetStreamInformation()), ex);
                }

                //Complete command async result if exist.
                _commandAsyncResultManager.TryComplete(command.Id, null, null);
            }
            else if (result == EventStreamPersistResult.Retried)
            {
                executed = true;
            }

            return executed;
        }
        private EventStreamPersistResult ProcessException(Exception exception, EventStream eventStream)
        {
            if (exception is ConcurrentException)
            {
                if (IsEventStreamCommitted(eventStream))
                {
                    return EventStreamPersistResult.Success;
                }

                var commandInfo = _processingCommandCache.Get(eventStream.CommandId);

                _logger.Error(string.Format(
                    "Concurrent exception raised when persisting event stream, command:{0}, event stream info:{1}",
                    commandInfo.Command.GetType().Name,
                    eventStream.GetStreamInformation()), exception);

                //Enforce to refresh memory cache from repository before retring the command
                //to ensure that when we retring the command, the memory cache is at the latest state.
                RefreshMemoryCacheFromRepository(eventStream);

                _retryCommandService.RetryCommand(commandInfo, exception);

                return EventStreamPersistResult.Retried;
            }
            else
            {
                var commandInfo = _processingCommandCache.Get(eventStream.CommandId);
                _logger.Error(string.Format(
                    "Unknown exception raised when persisting event stream, command:{0}, event stream info:{1}",
                    commandInfo.Command.GetType().Name,
                    eventStream.GetStreamInformation()), exception);
                return EventStreamPersistResult.UnknownException;
            }
        }
        private bool IsEventStreamCommitted(EventStream eventStream)
        {
            return _eventStore.IsEventStreamExist(
                eventStream.AggregateRootId,
                _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName),
                eventStream.Id);
        }
        private void RefreshMemoryCache(AggregateRoot aggregateRoot, EventStream eventStream)
        {
            aggregateRoot.AcceptEventStream(eventStream);
            _memoryCache.Set(aggregateRoot);
        }
        private void RefreshMemoryCacheFromRepository(EventStream eventStream)
        {
            var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName);
            var aggregateRoot = _repository.Get(aggregateRootType, eventStream.AggregateRootId);
            if (aggregateRoot != null)
            {
                _memoryCache.Set(aggregateRoot);
            }
        }

        enum EventStreamPersistResult
        {
            Success,
            Retried,
            UnknownException
        }
    }
}
