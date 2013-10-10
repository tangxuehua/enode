using System;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Retring;
using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>
    /// The default implementation of ICommittedEventExecutor.
    /// </summary>
    public class DefaultCommittedEventExecutor : MessageExecutor<EventStream>, ICommittedEventExecutor
    {
        #region Private Variables

        private readonly IEventHandlerProvider _eventHandlerProvider;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly IEventHandleInfoStore _eventHandleInfoStore;
        private readonly IEventHandleInfoCache _eventHandleInfoCache;
        private readonly IRetryService _retryService;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventHandlerProvider"></param>
        /// <param name="eventPublishInfoStore"></param>
        /// <param name="eventHandleInfoStore"></param>
        /// <param name="eventHandleInfoCache"></param>
        /// <param name="retryService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommittedEventExecutor(
            IEventHandlerProvider eventHandlerProvider,
            IEventPublishInfoStore eventPublishInfoStore,
            IEventHandleInfoStore eventHandleInfoStore,
            IEventHandleInfoCache eventHandleInfoCache,
            IRetryService retryService,
            ILoggerFactory loggerFactory)
        {
            _eventHandlerProvider = eventHandlerProvider;
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventHandleInfoStore = eventHandleInfoStore;
            _eventHandleInfoCache = eventHandleInfoCache;
            _retryService = retryService;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        /// <summary>Execute the given event stream.
        /// </summary>
        /// <param name="eventStream"></param>
        /// <param name="queue"></param>
        public override void Execute(EventStream eventStream, IMessageQueue<EventStream> queue)
        {
            TryDispatchEventsToEventHandlers(new EventStreamContext { EventStream = eventStream, Queue = queue });
        }

        #region Private Methods

        private void TryDispatchEventsToEventHandlers(EventStreamContext context)
        {
            Func<bool> tryDispatchEvents = () =>
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
            };

            try
            {
                _retryService.TryAction("TryDispatchEvents", tryDispatchEvents, 3, () => Clear(context));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when dispatching events:{0}", context.EventStream.GetStreamInformation()), ex);
            }
        }
        private bool DispatchEventsToHandlers(EventStream stream)
        {
            bool success = true;
            foreach (var evnt in stream.Events)
            {
                foreach (var handler in _eventHandlerProvider.GetEventHandlers(evnt.GetType()))
                {
                    if (!_retryService.TryRecursively("DispatchEventToHandler", () => DispatchEventToHandler(evnt, handler), 3))
                    {
                        success = false;
                    }
                }
            }
            if (success)
            {
                foreach (var evnt in stream.Events)
                {
                    _eventHandleInfoCache.RemoveEventHandleInfo(evnt.Id);
                }
            }
            return success;
        }
        private bool DispatchEventToHandler(IEvent evnt, IEventHandler handler)
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
                _logger.Error(string.Format("Exception raised when {0} handling {1}.", handler.GetInnerEventHandler().GetType().Name, evnt.GetType().Name), ex);
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
        private void Clear(EventStreamContext context)
        {
            UpdatePublishedEventStreamVersion(context.EventStream);
            FinishExecution(context.EventStream, context.Queue);
        }

        #endregion
    }
}
