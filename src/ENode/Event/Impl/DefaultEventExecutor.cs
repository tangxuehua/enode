using System;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class DefaultEventExecutor : IEventExecutor
    {
        private IEventHandlerProvider _eventHandlerProvider;
        private IEventPublishInfoStore _eventPublishInfoStore;
        private IEventHandleInfoStore _eventHandleInfoStore;
        private IEventPublisher _eventPublisher;
        private ILogger _logger;

        public DefaultEventExecutor(
            IEventHandlerProvider eventHandlerProvider,
            IEventPublishInfoStore eventPublishInfoStore,
            IEventHandleInfoStore eventHandleInfoStore,
            IEventPublisher eventPublisher,
            ILoggerFactory loggerFactory)
        {
            _eventHandlerProvider = eventHandlerProvider;
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventHandleInfoStore = eventHandleInfoStore;
            _eventPublisher = eventPublisher;
            _logger = loggerFactory.Create(GetType().Name);
        }

        public bool Execute(EventStream stream)
        {
            //If it is the first event stream of the aggregate, then do the event dispatching logic directly.
            if (stream.Version == 1)
            {
                var result = DoDispatchingLogic(stream);
                if (result == DispatchEventStreamResult.Success || result == DispatchEventStreamResult.RePublished)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            var eventPublishedVersion = _eventPublishInfoStore.GetEventPublishedVersion(stream.AggregateRootId);

            //If the last published event stream version + 1 is smaller than the current event stream version.
            //That means there must be some event streams which before the current event stream have not been published yet.
            //So in this case, we should retry to publish the event stream.
            if (eventPublishedVersion + 1 < stream.Version)
            {
                RePublishEventStream(stream);
                return true;
            }

            //If the current event stream is exactly the next event stream version, then do the event dispatching logic.
            if (eventPublishedVersion + 1 == stream.Version)
            {
                var result = DoDispatchingLogic(stream);
                if (result == DispatchEventStreamResult.Success || result == DispatchEventStreamResult.RePublished)
                {
                    return true;
                }
            }

            return false;
        }

        private DispatchEventStreamResult DoDispatchingLogic(EventStream stream)
        {
            var isDispatched = false;

            try
            {
                DispatchEventStreamToEventHandlers(stream);
                isDispatched = true;
                UpdatePublishedEventStreamVersion(stream);
                return DispatchEventStreamResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                if (!isDispatched)
                {
                    RePublishEventStream(stream);
                    return DispatchEventStreamResult.RePublished;
                }
                else
                {
                    return DispatchEventStreamResult.Failed;
                }
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
        private void DispatchEventStreamToEventHandlers(EventStream stream)
        {
            foreach (var evnt in stream.Events)
            {
                foreach (var eventHandler in _eventHandlerProvider.GetEventHandlers(evnt.GetType()))
                {
                    DispatchEventToEventHandler(eventHandler, evnt, 0, 3);
                }
            }
        }
        private void DispatchEventToEventHandler(IEventHandler eventHandler, IEvent evnt, int retriedCount, int maxRetryCount)
        {
            try
            {
                var eventHandlerTypeName = (eventHandler as IEventHandlerWrapper).GetInnerEventHandler().GetType().FullName;
                if (!_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeName))
                {
                    eventHandler.Handle(evnt);
                    _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, eventHandlerTypeName);
                }
            }
            catch (Exception ex)
            {
                LogError(eventHandler, evnt, ex, retriedCount + 1);
                if (retriedCount < maxRetryCount)
                {
                    DispatchEventToEventHandler(eventHandler, evnt, retriedCount + 1, maxRetryCount);
                }
            }
        }
        private void RePublishEventStream(EventStream stream)
        {
            _eventPublisher.Publish(stream);
        }
        private void LogError(IEventHandler eventHandler, IEvent evnt, Exception exception, int handleCount)
        {
            var eventHandlerType = (eventHandler as IEventHandlerWrapper).GetInnerEventHandler().GetType();
            _logger.Error(
                string.Format(
                    "Unknown exception raised when {0} handling {1}, handled count:{2}.",
                    eventHandlerType.Name,
                    evnt.GetType().Name,
                    handleCount),
                exception);
        }

        enum DispatchEventStreamResult
        {
            Success,
            RePublished,
            Failed
        }
    }
}
