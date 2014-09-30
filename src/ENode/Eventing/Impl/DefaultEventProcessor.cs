using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class DefaultEventProcessor : IEventProcessor
    {
        #region Private Variables

        private const int WorkerCount = 4;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly IEventHandlerTypeCodeProvider _eventHandlerTypeCodeProvider;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly IEventHandlerProvider _eventHandlerProvider;
        private readonly IProcessCommandSender _processCommandSender;
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
        /// <param name="eventTypeCodeProvider"></param>
        /// <param name="eventHandlerTypeCodeProvider"></param>
        /// <param name="commandTypeCodeProvider"></param>
        /// <param name="eventHandlerProvider"></param>
        /// <param name="processCommandSender"></param>
        /// <param name="repository"></param>
        /// <param name="eventPublishInfoStore"></param>
        /// <param name="eventHandleInfoStore"></param>
        /// <param name="eventHandleInfoCache"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultEventProcessor(
            IEventTypeCodeProvider eventTypeCodeProvider,
            IEventHandlerTypeCodeProvider eventHandlerTypeCodeProvider,
            ICommandTypeCodeProvider commandTypeCodeProvider,
            IEventHandlerProvider eventHandlerProvider,
            IProcessCommandSender processCommandSender,
            IRepository repository,
            IEventPublishInfoStore eventPublishInfoStore,
            IEventHandleInfoStore eventHandleInfoStore,
            IEventHandleInfoCache eventHandleInfoCache,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _eventTypeCodeProvider = eventTypeCodeProvider;
            _eventHandlerTypeCodeProvider = eventHandlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _eventHandlerProvider = eventHandlerProvider;
            _processCommandSender = processCommandSender;
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
            var queueIndex = processingContext.EventStream.CommandId.GetHashCode() % WorkerCount;
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
        private bool DispatchEventsCallback(object obj)
        {
            var context = obj as EventProcessingContext;
            context.EventProcessContext.OnEventProcessed(context.EventStream);
            return true;
        }
        private bool DispatchEventToHandler(string processId, IDictionary<string, string> contextItems, IEvent evnt, IEventHandler eventHandler)
        {
            try
            {
                var eventTypeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType());
                var eventHandlerType = eventHandler.GetInnerHandler().GetType();
                var eventHandlerTypeCode = _eventHandlerTypeCodeProvider.GetTypeCode(eventHandlerType);
                if (_eventHandleInfoCache.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;
                if (_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;

                var eventContext = new EventContext(_repository, contextItems);
                eventHandler.Handle(eventContext, evnt);
                var processCommands = eventContext.GetCommands();
                if (processCommands.Any())
                {
                    if (string.IsNullOrEmpty(processId))
                    {
                        throw new ENodeException("ProcessId cannot be null or empty if the event handler generates commands. eventHandlerType:{0}, eventType:{1}, eventId:{2}",
                            eventHandlerType.Name,
                            evnt.GetType().Name,
                            evnt.Id);
                    }
                    foreach (var processCommand in processCommands)
                    {
                        processCommand.Id = BuildCommandId(processCommand, evnt, eventHandlerTypeCode);
                        processCommand.Items["ProcessId"] = processId;
                        _processCommandSender.SendProcessCommand(processCommand, evnt.Id);
                        _logger.DebugFormat("Send process command success, commandType:{0}, commandId:{1}, eventHandlerType:{2}, eventType:{3}, eventId:{4}, processId:{5}",
                            processCommand.GetType().Name,
                            processCommand.Id,
                            eventHandlerType.Name,
                            evnt.GetType().Name,
                            evnt.Id,
                            processId);
                    }
                }
                _logger.DebugFormat("Handle event success. eventHandlerType:{0}, eventType:{1}, eventId:{2}",
                    eventHandlerType.Name,
                    evnt.GetType().Name,
                    evnt.Id);
                _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode, eventTypeCode, string.Empty, 0);
                _eventHandleInfoCache.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode, eventTypeCode, string.Empty, 0);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", eventHandler.GetInnerHandler().GetType().Name, evnt.GetType().Name), ex);
                return false;
            }
        }
        private string BuildCommandId(ICommand command, IEvent evnt, int eventHandlerTypeCode)
        {
            var key = command.GetKey();
            var commandKey = key == null ? string.Empty : key.ToString();
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}{1}{2}{3}", evnt.Id, commandKey, eventHandlerTypeCode, commandTypeCode);
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

            public EventContext(IRepository repository, IDictionary<string, string> items)
            {
                _repository = repository;
                Items = items ?? new Dictionary<string, string>();
            }

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
