using System;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventDispatcher : IEventDispatcher
    {
        #region Private Variables

        private readonly ParallelProcessor<IProcessingContext> _parallelProcessor;
        private readonly ITypeCodeProvider<IEvent> _eventTypeCodeProvider;
        private readonly ITypeCodeProvider<IEventHandler> _eventHandlerTypeCodeProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IHandlerProvider<IEventHandler> _eventHandlerProvider;
        private readonly ICommandService _commandService;
        private readonly IRepository _repository;
        private readonly IEventHandleInfoStore _eventHandleInfoStore;
        private readonly IEventHandleInfoCache _eventHandleInfoCache;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultEventDispatcher(
            ITypeCodeProvider<IEvent> eventTypeCodeProvider,
            ITypeCodeProvider<IEventHandler> eventHandlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IHandlerProvider<IEventHandler> eventHandlerProvider,
            ICommandService commandService,
            IRepository repository,
            IEventHandleInfoStore eventHandleInfoStore,
            IEventHandleInfoCache eventHandleInfoCache,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _parallelProcessor = new ParallelProcessor<IProcessingContext>(ENodeConfiguration.Instance.Setting.EventProcessorParallelThreadCount, "ProcessEvents", ProcessEvents);
            _eventTypeCodeProvider = eventTypeCodeProvider;
            _eventHandlerTypeCodeProvider = eventHandlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _eventHandlerProvider = eventHandlerProvider;
            _commandService = commandService;
            _repository = repository;
            _eventHandleInfoStore = eventHandleInfoStore;
            _eventHandleInfoCache = eventHandleInfoCache;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _parallelProcessor.Start();
        }
        public void EnqueueProcessingContext(IProcessingContext processingContext)
        {
            _parallelProcessor.EnqueueMessage(processingContext.GetHashKey(), processingContext);
        }
        public bool DispatchEventsToHandlers(EventStream eventStream)
        {
            var success = true;
            foreach (var evnt in eventStream.Events)
            {
                if (!DispatchEventToHandlers(evnt, false))
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
        public bool DispatchEventToHandlers(IEvent evnt)
        {
            return DispatchEventToHandlers(evnt, true);
        }

        #endregion

        #region Private Methods

        private void ProcessEvents(IProcessingContext context)
        {
            _actionExecutionService.TryAction(context.Name, context.Process, 3, new ActionInfo(context.Name + "Callback", context.Callback, null, null));
        }
        private bool DispatchEventToHandlers(IEvent evnt, bool autoRemoveEventHandleCache)
        {
            var success = true;
            foreach (var handler in _eventHandlerProvider.GetHandlers(evnt.GetType()))
            {
                if (!DispatchEventToHandler(evnt, handler))
                {
                    success = false;
                }
            }
            if (autoRemoveEventHandleCache)
            {
                _eventHandleInfoCache.RemoveEventHandleInfo(evnt.Id);
            }
            return success;
        }
        private bool DispatchEventToHandler(IEvent evnt, IEventHandler eventHandler)
        {
            var domainEvent = evnt as IDomainEvent;
            var eventTypeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType());
            var handlerType = eventHandler.GetInnerHandler().GetType();
            var handlerTypeCode = _eventHandlerTypeCodeProvider.GetTypeCode(handlerType);
            if (_eventHandleInfoCache.IsEventHandleInfoExist(evnt.Id, handlerTypeCode)) return true;
            if (_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, handlerTypeCode)) return true;
            var handlingContext = new DefaultHandlingContext(_repository);

            try
            {
                eventHandler.Handle(handlingContext, evnt);
                var commands = handlingContext.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, evnt, handlerTypeCode);
                        _commandService.Send(command, evnt.Id, "Event");

                        if (domainEvent != null)
                        {
                            _logger.DebugFormat("Send command success, commandType:{0}, commandId:{1}, eventHandlerType:{2}, eventType:{3}, eventId:{4}, eventVersion:{5}, sourceAggregateRootId:{6}",
                                command.GetType().Name,
                                command.Id,
                                handlerType.Name,
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
                                handlerType.Name,
                                evnt.GetType().Name,
                                evnt.Id);
                        }
                    }
                }

                if (domainEvent != null)
                {
                    _logger.DebugFormat("Handle event success. eventHandlerType:{0}, eventType:{1}, eventId:{2}, eventVersion:{3}, sourceAggregateRootId:{4}",
                        handlerType.Name,
                        domainEvent.GetType().Name,
                        domainEvent.Id,
                        domainEvent.Version,
                        domainEvent.AggregateRootId);
                    _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, handlerTypeCode, eventTypeCode, domainEvent.AggregateRootId, domainEvent.Version);
                    _eventHandleInfoCache.AddEventHandleInfo(evnt.Id, handlerTypeCode, eventTypeCode, domainEvent.AggregateRootId, domainEvent.Version);

                }
                else
                {
                    _logger.DebugFormat("Handle event success. eventHandlerType:{0}, eventType:{1}, eventId:{2}",
                        handlerType.Name,
                        evnt.GetType().Name,
                        evnt.Id);
                    _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, handlerTypeCode, eventTypeCode, string.Empty, 0);
                    _eventHandleInfoCache.AddEventHandleInfo(evnt.Id, handlerTypeCode, eventTypeCode, string.Empty, 0);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", handlerType.Name, evnt.GetType().Name), ex);
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
    }
}
