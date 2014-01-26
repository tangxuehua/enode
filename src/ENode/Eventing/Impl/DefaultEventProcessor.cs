using System;
using ECommon.Logging;
using ECommon.Retring;

namespace ENode.Eventing.Impl
{
    public class DefaultEventProcessor : IEventProcessor
    {
        #region Private Variables

        private readonly IEventHandlerProvider _eventHandlerProvider;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly IEventHandleInfoStore _eventHandleInfoStore;
        private readonly IEventHandleInfoCache _eventHandleInfoCache;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventHandlerProvider"></param>
        /// <param name="eventPublishInfoStore"></param>
        /// <param name="eventHandleInfoStore"></param>
        /// <param name="eventHandleInfoCache"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultEventProcessor(
            IEventHandlerProvider eventHandlerProvider,
            IEventPublishInfoStore eventPublishInfoStore,
            IEventHandleInfoStore eventHandleInfoStore,
            IEventHandleInfoCache eventHandleInfoCache,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _eventHandlerProvider = eventHandlerProvider;
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventHandleInfoStore = eventHandleInfoStore;
            _eventHandleInfoCache = eventHandleInfoCache;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        public void Process(EventStream eventStream, IEventProcessContext context)
        {
            DispatchEventsToHandlers(new EventProcessingContext(eventStream, context));
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
                        if (lastPublishedVersion + 1 == eventStream.Version)
                        {
                            return DispatchEventsToHandlers(eventStream);
                        }
                        return lastPublishedVersion + 1 > eventStream.Version;
                }
            });

            try
            {
                _actionExecutionService.TryAction("DispatchEventsToHandlers", dispatchEventsToHandlers, 3, new ActionInfo("DispatchEventsToHandlersCallback", obj =>
                {
                    var currentContext = obj as EventProcessingContext;
                    UpdatePublishedEventStreamVersion(currentContext.EventStream);
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
                var eventHandlerTypeName = handler.GetInnerEventHandler().GetType().FullName;
                if (_eventHandleInfoCache.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeName)) return true;
                if (_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeName)) return true;

                handler.Handle(evnt);
                _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, eventHandlerTypeName);
                _eventHandleInfoCache.AddEventHandleInfo(evnt.Id, eventHandlerTypeName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", handler.GetInnerEventHandler().GetType().Name, evnt.GetType().Name), ex);
                return false;
            }
        }
        private void UpdatePublishedEventStreamVersion(EventStream stream)
        {
            if (stream.Version == 1)
            {
                _eventPublishInfoStore.InsertFirstPublishedVersion(stream.AggregateRootId);
            }
            else
            {
                _eventPublishInfoStore.UpdatePublishedVersion(stream.AggregateRootId, stream.Version);
            }
        }

        #endregion
    }
}
