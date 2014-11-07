using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventProcessor : IMessageProcessor<IDomainEventStream>, IMessageProcessor<IEventStream>, IMessageProcessor<IEvent>
    {
        #region Private Variables

        private int _workerCount = 4;
        private readonly ITypeCodeProvider<IEvent> _eventTypeCodeProvider;
        private readonly ITypeCodeProvider<IEventHandler> _eventHandlerTypeCodeProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IMessageHandlerProvider<IEventHandler> _eventHandlerProvider;
        private readonly ICommandService _commandService;
        private readonly IRepository _repository;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly IEventHandleInfoStore _eventHandleInfoStore;
        private readonly IEventHandleInfoCache _eventHandleInfoCache;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;
        private readonly IList<BlockingCollection<IProcessingContext>> _queueList;
        private readonly IList<Worker> _workerList;
        private bool _isStarted;

        #endregion

        public string Name { get; set; }

        #region Constructors

        public DefaultEventProcessor(
            ITypeCodeProvider<IEvent> eventTypeCodeProvider,
            ITypeCodeProvider<IEventHandler> eventHandlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IMessageHandlerProvider<IEventHandler> eventHandlerProvider,
            ICommandService commandService,
            IRepository repository,
            IEventPublishInfoStore eventPublishInfoStore,
            IEventHandleInfoStore eventHandleInfoStore,
            IEventHandleInfoCache eventHandleInfoCache,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            Name = GetType().Name;
            _eventTypeCodeProvider = eventTypeCodeProvider;
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
            _queueList = new List<BlockingCollection<IProcessingContext>>();
            _workerCount = ENodeConfiguration.Instance.Setting.EventProcessorParallelThreadCount;
            _workerList = new List<Worker>();
            for (var index = 0; index < _workerCount; index++)
            {
                _queueList.Add(new BlockingCollection<IProcessingContext>());
            }
        }

        #endregion

        public void Start()
        {
            if (_isStarted) return;

            for (var index = 0; index < _workerCount; index++)
            {
                var queue = _queueList[index];
                var worker = new Worker("ProcessEvents", () =>
                {
                    ProcessEvents(queue.Take());
                });
                _workerList.Add(worker);
                worker.Start();
            }
            _isStarted = true;
        }
        public void Process(IDomainEventStream domainEventStream, IMessageProcessContext<IDomainEventStream> context)
        {
            QueueProcessingContext(domainEventStream.AggregateRootId, new DomainEventStreamProcessingContext(this, domainEventStream, context));
        }
        public void Process(IEventStream eventStream, IMessageProcessContext<IEventStream> context)
        {
            QueueProcessingContext(eventStream.CommandId, new EventStreamProcessingContext(this, eventStream, context));
        }
        public void Process(IEvent evnt, IMessageProcessContext<IEvent> context)
        {
            QueueProcessingContext(evnt.Id, new EventProcessingContext(this, evnt, context));
        }

        #region Private Methods

        private void QueueProcessingContext(object hashKey, IProcessingContext processingContext)
        {
            var queueIndex = hashKey.GetHashCode() % _workerCount;
            if (queueIndex < 0)
            {
                queueIndex = Math.Abs(queueIndex);
            }
            _queueList[queueIndex].Add(processingContext);
        }
        private void ProcessEvents(IProcessingContext context)
        {
            _actionExecutionService.TryAction(context.ProcessName, context.Process, 3, new ActionInfo(context.ProcessName + "Callback", context.ProcessCallback, null, null));
        }
        private bool DispatchEventsToHandlers(IEventStream eventStream)
        {
            var success = true;
            foreach (var evnt in eventStream.Events)
            {
                if (!DispatchEventToHandlers(evnt))
                {
                    success = false;
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
        private bool DispatchEventToHandlers(IEvent evnt)
        {
            var success = true;
            foreach (var handler in _eventHandlerProvider.GetMessageHandlers(evnt.GetType()))
            {
                if (!DispatchEventToHandler(evnt, handler))
                {
                    success = false;
                }
            }
            return success;
        }
        private bool DispatchEventToHandler(IEvent evnt, IEventHandler eventHandler)
        {
            try
            {
                var domainEvent = evnt as IDomainEvent;
                var eventTypeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType());
                var eventHandlerType = eventHandler.GetInnerHandler().GetType();
                var eventHandlerTypeCode = _eventHandlerTypeCodeProvider.GetTypeCode(eventHandlerType);
                if (_eventHandleInfoCache.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;
                if (_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeCode)) return true;

                var eventContext = new EventContext(_repository);
                eventHandler.Handle(eventContext, evnt);
                var commands = eventContext.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, evnt, eventHandlerTypeCode);
                        _commandService.Send(command, evnt.Id, null);

                        if (domainEvent != null)
                        {
                            _logger.DebugFormat("Send command success, commandType:{0}, commandId:{1}, eventHandlerType:{2}, eventType:{3}, eventId:{4}, eventVersion:{5}, sourceAggregateRootId:{6}",
                                command.GetType().Name,
                                command.Id,
                                eventHandlerType.Name,
                                domainEvent.GetType().Name,
                                domainEvent.Id,
                                domainEvent.Version,
                                domainEvent.AggregateRootId);
                        }
                        else
                        {
                            _logger.DebugFormat("Send command success, commandType:{0}, commandId:{1}, eventHandlerType:{2}, eventType:{3}, eventId:{4}",
                                command.GetType().Name,
                                command.Id,
                                eventHandlerType.Name,
                                evnt.GetType().Name,
                                evnt.Id);
                        }
                    }
                }

                if (domainEvent != null)
                {
                    _logger.DebugFormat("Handle event success. eventHandlerType:{0}, eventType:{1}, eventId:{2}, eventVersion:{3}, sourceAggregateRootId:{4}",
                        eventHandlerType.Name,
                        domainEvent.GetType().Name,
                        domainEvent.Id,
                        domainEvent.Version,
                        domainEvent.AggregateRootId);
                    _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode, eventTypeCode, domainEvent.AggregateRootId, domainEvent.Version);
                    _eventHandleInfoCache.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode, eventTypeCode, domainEvent.AggregateRootId, domainEvent.Version);
                
                }
                else
                {
                    _logger.DebugFormat("Handle event success. eventHandlerType:{0}, eventType:{1}, eventId:{2}",
                        eventHandlerType.Name,
                        evnt.GetType().Name,
                        evnt.Id);
                    _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode, eventTypeCode, string.Empty, 0);
                    _eventHandleInfoCache.AddEventHandleInfo(evnt.Id, eventHandlerTypeCode, eventTypeCode, string.Empty, 0);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", eventHandler.GetInnerHandler().GetType().Name, evnt.GetType().Name), ex);
                return false;
            }
        }
        private void UpdatePublishedVersion(IDomainEventStream stream)
        {
            if (stream.Version == 1)
            {
                _eventPublishInfoStore.InsertPublishedVersion(Name, stream.AggregateRootId);
            }
            else
            {
                _eventPublishInfoStore.UpdatePublishedVersion(Name, stream.AggregateRootId, stream.Version);
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

        class DomainEventStreamProcessingContext : ProcessingContext<IDomainEventStream>
        {
            private DefaultEventProcessor _processor;

            public DomainEventStreamProcessingContext(DefaultEventProcessor processor, IDomainEventStream domainEventStream, IMessageProcessContext<IDomainEventStream> eventProcessContext)
                : base("ProcessDomainEventStream", domainEventStream, eventProcessContext)
            {
                _processor = processor;
            }
            public override bool Process()
            {
                var domainEventStream = Message;
                var lastPublishedVersion = _processor._eventPublishInfoStore.GetEventPublishedVersion(_processor.Name, domainEventStream.AggregateRootId);

                if (lastPublishedVersion + 1 == domainEventStream.Version)
                {
                    return _processor.DispatchEventsToHandlers(domainEventStream);
                }
                else if (lastPublishedVersion + 1 < domainEventStream.Version)
                {
                    _processor._logger.DebugFormat("Wait to publish, [aggregateRootId={0},lastPublishedVersion={1},currentVersion={2}]", domainEventStream.AggregateRootId, lastPublishedVersion, domainEventStream.Version);
                    return false;
                }

                return true;
            }
            protected override void OnMessageProcessed()
            {
                _processor.UpdatePublishedVersion(Message);
                base.OnMessageProcessed();
            }
        }
        class EventStreamProcessingContext : ProcessingContext<IEventStream>
        {
            private DefaultEventProcessor _processor;

            public EventStreamProcessingContext(DefaultEventProcessor processor, IEventStream eventStream, IMessageProcessContext<IEventStream> eventProcessContext)
                : base("ProcessEventStream", eventStream, eventProcessContext)
            {
                _processor = processor;
            }
            public override bool Process()
            {
                return _processor.DispatchEventsToHandlers(Message);
            }
        }
        class EventProcessingContext : ProcessingContext<IEvent>
        {
            private DefaultEventProcessor _processor;

            public EventProcessingContext(DefaultEventProcessor processor, IEvent evnt, IMessageProcessContext<IEvent> eventProcessContext)
                : base("ProcessEvent", evnt, eventProcessContext)
            {
                _processor = processor;
            }
            public override bool Process()
            {
                var success = _processor.DispatchEventToHandlers(Message);
                if (success)
                {
                    _processor._eventHandleInfoCache.RemoveEventHandleInfo(Message.Id);
                }
                return success;
            }
        }
        class EventContext : IEventContext
        {
            private readonly List<ICommand> _commands = new List<ICommand>();
            private readonly IRepository _repository;

            public EventContext(IRepository repository)
            {
                _repository = repository;
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
