using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;
using ENode.Commanding;

namespace ENode.Eventing.Impl
{
    public class DefaultEventProcessor : IEventProcessor
    {
        #region Private Variables

        private const int WorkerCount = 4;
        private readonly IEventHandlerTypeCodeProvider _eventHandlerTypeCodeProvider;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly IEventHandlerProvider _eventHandlerProvider;
        private readonly ICommandService _commandService;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly IEventHandleInfoStore _eventHandleInfoStore;
        private readonly IEventHandleInfoCache _eventHandleInfoCache;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;
        private readonly IList<BlockingCollection<EventProcessingContext>> _queueList;
        private readonly IList<Worker> _workerList;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventHandlerTypeCodeProvider"></param>
        /// <param name="eventHandlerProvider"></param>
        /// <param name="eventPublishInfoStore"></param>
        /// <param name="eventHandleInfoStore"></param>
        /// <param name="eventHandleInfoCache"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultEventProcessor(
            IEventHandlerTypeCodeProvider eventHandlerTypeCodeProvider,
            ICommandTypeCodeProvider commandTypeCodeProvider,
            IEventHandlerProvider eventHandlerProvider,
            ICommandService commandService,
            IEventPublishInfoStore eventPublishInfoStore,
            IEventHandleInfoStore eventHandleInfoStore,
            IEventHandleInfoCache eventHandleInfoCache,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _eventHandlerTypeCodeProvider = eventHandlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _eventHandlerProvider = eventHandlerProvider;
            _commandService = commandService;
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventHandleInfoStore = eventHandleInfoStore;
            _eventHandleInfoCache = eventHandleInfoCache;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().Name);
            _queueList = new List<BlockingCollection<EventProcessingContext>>();
            for (var index = 0; index < WorkerCount; index++)
            {
                _queueList.Add(new BlockingCollection<EventProcessingContext>(new ConcurrentQueue<EventProcessingContext>()));
            }

            _workerList = new List<Worker>();
            for (var index = 0; index < WorkerCount; index++)
            {
                var queue = _queueList[index];
                var worker = new Worker(() =>
                {
                    DispatchEventsToHandlers(queue.Take());
                });
                _workerList.Add(worker);
                worker.Start();
            }
        }

        #endregion

        public void Process(EventStream eventStream, IEventProcessContext context)
        {
            var processingContext = new EventProcessingContext(eventStream, context);
            var queueIndex = processingContext.EventStream.AggregateRootId.GetHashCode() % WorkerCount;
            if (queueIndex < 0)
            {
                queueIndex = Math.Abs(queueIndex);
            }
            _queueList[queueIndex].Add(processingContext);
        }

        #region Private Methods

        private void DispatchEventsToHandlers(EventProcessingContext context)
        {
            var dispatchEventsToHandlers = new Func<bool>(() =>
            {
                var eventStream = context.EventStream;
                switch (eventStream.Version)
                {
                    case 1:
                        return DispatchEventsToHandlers(eventStream);
                    default:
                        var lastPublishedVersion = _eventPublishInfoStore.GetEventPublishedVersion(eventStream.AggregateRootId);
                        if (lastPublishedVersion == eventStream.Version - 1)
                        {
                            return DispatchEventsToHandlers(eventStream);
                        }
                        if (lastPublishedVersion < eventStream.Version - 1)
                        {
                            _logger.DebugFormat("wait to publish, [aggregateRootId={0},lastPublishedVersion={1},currentVersion={2}]", eventStream.AggregateRootId, lastPublishedVersion, eventStream.Version);
                            return false;
                        }
                        return true;
                }
            });

            try
            {
                _actionExecutionService.TryAction("DispatchEventsToHandlers", dispatchEventsToHandlers, 5, new ActionInfo("DispatchEventsToHandlersCallback", obj =>
                {
                    var currentContext = obj as EventProcessingContext;
                    UpdatePublishedVersion(currentContext.EventStream);
                    currentContext.EventProcessContext.OnEventProcessed(currentContext.EventStream);
                    return true;
                }, context, null));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when dispatching event stream:{0}", context.EventStream), ex);
            }
        }
        private bool DispatchEventsToHandlers(EventStream eventStream)
        {
            var success = true;
            foreach (var evnt in eventStream.Events)
            {
                foreach (var handler in _eventHandlerProvider.GetEventHandlers(evnt.GetType()))
                {
                    if (!_actionExecutionService.TryRecursively("DispatchEventToHandler", () => DispatchEventToHandler(evnt, handler), 3))
                    {
                        success = false;
                    }
                }
            }
            if (success)
            {
                foreach (var evnt in eventStream.Events)
                {
                    _eventHandleInfoCache.RemoveEventHandleInfo(evnt.Id);
                }
            }
            return success;
        }
        private bool DispatchEventToHandler(IDomainEvent evnt, IEventHandler handler)
        {
            try
            {
                var eventHandlerTypeCode = _eventHandlerTypeCodeProvider.GetTypeCode(handler.GetInnerEventHandler().GetType());
                if (_eventHandleInfoCache.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;
                if (_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;

                var context = new EventContext();
                handler.Handle(context, evnt);
                var commands = context.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, evnt, eventHandlerTypeCode);
                        _commandService.Send(command);
                    }
                }
                _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode);
                _eventHandleInfoCache.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", handler.GetInnerEventHandler().GetType().Name, evnt.GetType().Name), ex);
                return false;
            }
        }
        private void UpdatePublishedVersion(EventStream stream)
        {
            if (stream.Version == 1)
            {
                _eventPublishInfoStore.InsertPublishedVersion(stream.AggregateRootId);
            }
            else
            {
                _eventPublishInfoStore.UpdatePublishedVersion(stream.AggregateRootId, stream.Version);
            }
        }
        private string BuildCommandId(ICommand command, IDomainEvent evnt, int eventHandlerTypeCode)
        {
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}_{1}_{2}", evnt.Id, eventHandlerTypeCode, commandTypeCode);
        }

        #endregion

        class EventProcessingContext
        {
            public EventStream EventStream { get; private set; }
            public IEventProcessContext EventProcessContext { get; private set; }

            public EventProcessingContext(EventStream eventStream, IEventProcessContext eventProcessContext)
            {
                EventStream = eventStream;
                EventProcessContext = eventProcessContext;
            }
        }
    }
}
