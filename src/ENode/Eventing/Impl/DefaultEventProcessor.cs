using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Domain;

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
        private readonly IRepository _repository;
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
        /// <param name="commandTypeCodeProvider"></param>
        /// <param name="eventHandlerProvider"></param>
        /// <param name="commandService"></param>
        /// <param name="repository"></param>
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
            IRepository repository,
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
            _repository = repository;
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventHandleInfoStore = eventHandleInfoStore;
            _eventHandleInfoCache = eventHandleInfoCache;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().FullName);
            _queueList = new List<BlockingCollection<EventProcessingContext>>();
            for (var index = 0; index < WorkerCount; index++)
            {
                _queueList.Add(new BlockingCollection<EventProcessingContext>(new ConcurrentQueue<EventProcessingContext>()));
            }

            _workerList = new List<Worker>();
            for (var index = 0; index < WorkerCount; index++)
            {
                var queue = _queueList[index];
                var worker = new Worker("ProcessEvents", () =>
                {
                    ProcessEvents(queue.Take());
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

        private void ProcessEvents(EventProcessingContext context)
        {
            _actionExecutionService.TryAction(
                "DispatchEvents",
                () => DispatchEvents(context),
                3,
                new ActionInfo("DispatchEventsCallback", DispatchEventsCallback, context, null));
        }
        private bool DispatchEvents(EventProcessingContext context)
        {
            var eventStream = context.EventStream;
            if (eventStream.Version == 1)
            {
                return DispatchEventsToHandlers(eventStream);
            }

            var lastPublishedVersion = _eventPublishInfoStore.GetEventPublishedVersion(eventStream.AggregateRootId);
            if (lastPublishedVersion == eventStream.Version - 1)
            {
                return DispatchEventsToHandlers(eventStream);
            }
            else if (lastPublishedVersion < eventStream.Version - 1)
            {
                _logger.DebugFormat("Wait to publish, [aggregateRootId={0},lastPublishedVersion={1},currentVersion={2}]", eventStream.AggregateRootId, lastPublishedVersion, eventStream.Version);
                return false;
            }

            return true;
        }
        private bool DispatchEventsCallback(object obj)
        {
            var context = obj as EventProcessingContext;
            UpdatePublishedVersion(context.EventStream);
            context.EventProcessContext.OnEventProcessed(context.EventStream);
            return true;
        }
        private bool DispatchEventsToHandlers(EventStream eventStream)
        {
            var success = true;
            foreach (var evnt in eventStream.Events)
            {
                foreach (var handler in _eventHandlerProvider.GetEventHandlers(evnt.GetType()))
                {
                    if (!DispatchEventToHandler(eventStream.ProcessId, eventStream.Items, evnt, handler))
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
        private bool DispatchEventToHandler(string processId, IDictionary<string, string> items, IDomainEvent evnt, IEventHandler handler)
        {
            try
            {
                var eventHandlerType = handler.GetInnerEventHandler().GetType();
                var eventHandlerTypeCode = _eventHandlerTypeCodeProvider.GetTypeCode(eventHandlerType);
                if (_eventHandleInfoCache.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;
                if (_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;

                var context = new EventContext(_repository, processId, items);
                handler.Handle(context, evnt);
                var commands = context.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, evnt, eventHandlerTypeCode);
                        _commandService.Send(command);
                        _logger.DebugFormat("Send command from event context success. eventHandlerType:{0}, eventType:{1}, eventId:{2}, eventVersion:{3}, sourceAggregateRootId:{4}, commandType:{5}, commandId:{6}",
                            eventHandlerType.Name,
                            evnt.GetType().Name,
                            evnt.Id,
                            evnt.Version,
                            evnt.AggregateRootId,
                            command.GetType().Name,
                            command.Id);
                    }
                }
                _logger.DebugFormat("Handle event success. eventHandlerType:{0}, eventType:{1}, eventId:{2}, eventVersion:{3}, sourceAggregateRootId:{4}",
                    eventHandlerType.Name,
                    evnt.GetType().Name,
                    evnt.Id,
                    evnt.Version,
                    evnt.AggregateRootId);
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
            var key = command.GetKey();
            var commandKey = key == null ? string.Empty : key.ToString();
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}{1}{2}{3}", evnt.Id, eventHandlerTypeCode, commandTypeCode, commandKey);
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
        class EventContext : IEventContext
        {
            private readonly List<ICommand> _commands = new List<ICommand>();
            private readonly IRepository _repository;

            public EventContext(IRepository repository, string processId, IDictionary<string, string> items)
            {
                _repository = repository;
                ProcessId = processId;
                Items = items ?? new Dictionary<string, string>();
            }

            public string ProcessId { get; private set; }
            public IDictionary<string, string> Items { get; private set; }

            public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
            {
                return _repository.Get<T>(aggregateRootId);
            }
            public void AddCommand(ICommand command)
            {
                _commands.Add(command);
            }
            public IEnumerable<ICommand> GetCommands()
            {
                return _commands;
            }
        }
    }
}
