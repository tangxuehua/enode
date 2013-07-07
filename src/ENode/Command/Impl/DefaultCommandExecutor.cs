using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Messaging;

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
        private IEventPersistenceSynchronizerProvider _eventPersistenceSynchronizerProvider;
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
            IEventPersistenceSynchronizerProvider eventPersistenceSynchronizerProvider,
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
            _eventPersistenceSynchronizerProvider = eventPersistenceSynchronizerProvider;
            _commandContext = commandContext;
            _trackingContext = commandContext as ITrackingContext;
            _logger = loggerFactory.Create(GetType().Name);

            if (_trackingContext == null)
            {
                throw new Exception("command context must also implement ITrackingContext interface.");
            }
        }

        #endregion

        public MessageExecuteResult Execute(ICommand command)
        {
            var executeResult = MessageExecuteResult.None;
            var errorInfo = new ErrorInfo();

            var commandHandler = _commandHandlerProvider.GetCommandHandler(command);
            if (commandHandler == null)
            {
                var errorMessage = string.Format("Command handler not found for {0}", command.GetType().FullName);
                _logger.Fatal(errorMessage);
                _commandAsyncResultManager.TryComplete(command.Id, errorMessage, null);
                return MessageExecuteResult.Executed;
            }

            var submitResult = SubmitResult.None;
            AggregateRoot dirtyAggregate = null;

            try
            {
                _trackingContext.Clear();
                _processingCommandCache.Add(command);
                commandHandler.Handle(_commandContext, command);
                dirtyAggregate = GetDirtyAggregate(_trackingContext);
                if (dirtyAggregate != null)
                {
                    submitResult = SubmitChanges(dirtyAggregate, BuildEventStream(dirtyAggregate, command), command, errorInfo);
                }
            }
            catch (Exception ex)
            {
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                errorInfo.ErrorMessage = string.Format("Exception raised when {0} handling {1}, command id:{2}.", commandHandlerType.Name, command.GetType().Name, command.Id);
                errorInfo.Exception = ex;
                _logger.Error(errorInfo.ErrorMessage, ex);
            }
            finally
            {
                _trackingContext.Clear();
                _processingCommandCache.TryRemove(command.Id);

                if (dirtyAggregate == null)
                {
                    _commandAsyncResultManager.TryComplete(command.Id, errorInfo.ErrorMessage, errorInfo.Exception);
                    executeResult = MessageExecuteResult.Executed;
                }
                else
                {
                    if (submitResult == SubmitResult.None ||
                        submitResult == SubmitResult.Success ||
                        submitResult == SubmitResult.SynchronizerFailed)
                    {
                        _commandAsyncResultManager.TryComplete(command.Id, errorInfo.ErrorMessage, errorInfo.Exception);
                        executeResult = MessageExecuteResult.Executed;
                    }
                    else if (submitResult == SubmitResult.Retried)
                    {
                        executeResult = MessageExecuteResult.Executed;
                    }
                    else if (submitResult == SubmitResult.PublishFailed || submitResult == SubmitResult.Failed)
                    {
                        executeResult = MessageExecuteResult.Failed;
                    }
                }
            }

            return executeResult;
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
        private SubmitResult SubmitChanges(AggregateRoot aggregateRoot, EventStream eventStream, ICommand command, ErrorInfo errorInfo)
        {
            var submitResult = SubmitResult.None;

            var synchronizers = _eventPersistenceSynchronizerProvider.GetSynchronizers(eventStream);
            var success = TryCallSynchronizersBeforeEventPersisting(synchronizers, eventStream, errorInfo);
            if (!success)
            {
                return SubmitResult.SynchronizerFailed;
            }

            var persistResult = PersistResult.None;
            try
            {
                _eventStore.Append(eventStream);
                persistResult = PersistResult.Success;
            }
            catch (Exception ex)
            {
                persistResult = ProcessException(ex, eventStream, errorInfo);
            }

            if (persistResult == PersistResult.Success)
            {
                TryRefreshMemoryCache(aggregateRoot, eventStream);
                TryCallSynchronizersAfterEventPersisted(synchronizers, eventStream);
                if (TryPublishEventStream(eventStream))
                {
                    submitResult = SubmitResult.Success;
                }
                else
                {
                    submitResult = SubmitResult.PublishFailed;
                }
            }
            else if (persistResult == PersistResult.Retried)
            {
                submitResult = SubmitResult.Retried;
            }
            else if (persistResult == PersistResult.Failed)
            {
                submitResult = SubmitResult.Failed;
            }

            return submitResult;
        }
        private PersistResult ProcessException(Exception exception, EventStream eventStream, ErrorInfo errorInfo)
        {
            if (exception is ConcurrentException)
            {
                if (IsEventStreamCommitted(eventStream))
                {
                    return PersistResult.Success;
                }

                var commandInfo = _processingCommandCache.Get(eventStream.CommandId);

                _logger.Error(string.Format(
                    "Concurrent exception raised when persisting event stream, command:{0}, event stream:{1}",
                    commandInfo.Command.GetType().Name,
                    eventStream.GetStreamInformation()), exception);

                _retryCommandService.RetryCommand(commandInfo, exception);

                return PersistResult.Retried;
            }
            else
            {
                var commandInfo = _processingCommandCache.Get(eventStream.CommandId);
                _logger.Error(string.Format(
                    "Unknown exception raised when persisting event stream, command:{0}, event stream:{1}",
                    commandInfo.Command.GetType().Name,
                    eventStream.GetStreamInformation()), exception);
                return PersistResult.Failed;
            }
        }
        private bool IsEventStreamCommitted(EventStream eventStream)
        {
            return _eventStore.IsEventStreamExist(
                eventStream.AggregateRootId,
                _aggregateRootTypeProvider.GetAggregateRootType(eventStream.AggregateRootName),
                eventStream.Id);
        }
        private void TryRefreshMemoryCache(AggregateRoot aggregateRoot, EventStream eventStream)
        {
            try
            {
                aggregateRoot.AcceptEventStream(eventStream);
                _memoryCache.Set(aggregateRoot);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Unknown exception raised when refreshing memory cache by event stream:{0}", eventStream.GetStreamInformation()), ex);
            }
        }
        private bool TryPublishEventStream(EventStream eventStream)
        {
            var success = false;
            try
            {
                _eventPublisher.Publish(eventStream);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Unknown exception raised when publishing event stream:{0}", eventStream.GetStreamInformation()), ex);
            }
            return success;
        }
        private bool TryCallSynchronizersBeforeEventPersisting(IEnumerable<IEventPersistenceSynchronizer> synchronizers, EventStream eventStream, ErrorInfo errorInfo)
        {
            if (synchronizers != null && synchronizers.Count() > 0)
            {
                foreach (var synchronizer in synchronizers)
                {
                    try
                    {
                        synchronizer.OnBeforePersisting(eventStream);
                    }
                    catch (Exception ex)
                    {
                        var commandInfo = _processingCommandCache.Get(eventStream.CommandId);
                        errorInfo.Exception = ex;
                        errorInfo.ErrorMessage = string.Format(
                            "Unknown exception raised when calling synchronizer's OnBeforePersisting method. synchronizer:{0}, command:{1}, event stream:{2}",
                            synchronizer.GetType().Name,
                            commandInfo.Command.GetType().Name,
                            eventStream.GetStreamInformation());
                        _logger.Error(errorInfo.ErrorMessage, ex);
                        return false;
                    }
                }
            }

            return true;
        }
        private void TryCallSynchronizersAfterEventPersisted(IEnumerable<IEventPersistenceSynchronizer> synchronizers, EventStream eventStream)
        {
            if (synchronizers != null && synchronizers.Count() > 0)
            {
                foreach (var synchronizer in synchronizers)
                {
                    try
                    {
                        synchronizer.OnAfterPersisted(eventStream);
                    }
                    catch (Exception ex)
                    {
                        var commandInfo = _processingCommandCache.Get(eventStream.CommandId);
                        _logger.Error(string.Format(
                            "Unknown exception raised when calling synchronizer's OnAfterPersisted method. synchronizer:{0}, command:{1}, event stream:{2}",
                            synchronizer.GetType().Name,
                            commandInfo.Command.GetType().Name,
                            eventStream.GetStreamInformation()), ex);
                    }
                }
            }
        }

        class ErrorInfo
        {
            public string ErrorMessage { get; set; }
            public Exception Exception { get; set; }
        }
        enum SubmitResult
        {
            None,
            Success,
            Retried,
            SynchronizerFailed,
            PublishFailed,
            Failed
        }
        enum PersistResult
        {
            None,
            Success,
            Retried,
            Failed
        }
    }
}
