using System;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Retring;
using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultCommittedEventExecutor : MessageExecutor<EventStream>, ICommittedEventExecutor
    {
        #region Private Variables

        private readonly IEventHandlerProvider _eventHandlerProvider;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly IEventHandleInfoStore _eventHandleInfoStore;
        private readonly IRetryService _retryService;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventHandlerProvider"></param>
        /// <param name="eventPublishInfoStore"></param>
        /// <param name="eventHandleInfoStore"></param>
        /// <param name="retryService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommittedEventExecutor(
            IEventHandlerProvider eventHandlerProvider,
            IEventPublishInfoStore eventPublishInfoStore,
            IEventHandleInfoStore eventHandleInfoStore,
            IRetryService retryService,
            ILoggerFactory loggerFactory)
        {
            _eventHandlerProvider = eventHandlerProvider;
            _eventPublishInfoStore = eventPublishInfoStore;
            _eventHandleInfoStore = eventHandleInfoStore;
            _retryService = retryService;
            _logger = loggerFactory.Create(GetType().Name);
        }

        #endregion

        /// <summary>
        /// 
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
            Func<EventStreamContext, bool> tryDispatchEventsAction = (streamContext) =>
            {
                switch (streamContext.EventStream.Version)
                {
                    case 1:
                        return DispatchEventsToHandlers(streamContext.EventStream);
                    default:
                    {
                        var lastPublishedVersion = _eventPublishInfoStore.GetEventPublishedVersion(streamContext.EventStream.AggregateRootId);

                        if (lastPublishedVersion + 1 == streamContext.EventStream.Version)
                        {
                            return DispatchEventsToHandlers(streamContext.EventStream);
                        }
                        return lastPublishedVersion + 1 > streamContext.EventStream.Version;
                    }
                }
            };

            try
            {
                if (_retryService.TryAction("TryDispatchEvents", () => tryDispatchEventsAction(context), 3))
                {
                    Clear(context);
                }
                else
                {
                    _retryService.RetryInQueue(
                        new ActionInfo(
                            "TryDispatchEvents",
                            (obj) => tryDispatchEventsAction(obj as EventStreamContext),
                            context,
                            new ActionInfo(
                                "DispatchEventsSuccessAction",
                                (data) => { Clear(data as EventStreamContext); return true; },
                                context, null)
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when dispatching events:{0}", context.EventStream.GetStreamInformation()), ex);
            }
        }
        private bool DispatchEventsToHandlers(EventStream stream)
        {
            foreach (var evnt in stream.Events)
            {
                foreach (var handler in _eventHandlerProvider.GetEventHandlers(evnt.GetType()))
                {
                    var evnt1 = evnt;
                    var handler1 = handler;
                    var success = _retryService.TryAction("DispatchEventToHandler", () => DispatchEventToHandler(evnt1, handler1), 2);
                    if (!success)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool DispatchEventToHandler(IEvent evnt, IEventHandler handler)
        {
            try
            {
                var eventHandlerTypeName = handler.GetInnerEventHandler().GetType().FullName;
                if (_eventHandleInfoStore.IsEventHandleInfoExist(evnt.Id, eventHandlerTypeName)) return true;

                handler.Handle(evnt);
                _eventHandleInfoStore.AddEventHandleInfo(evnt.Id, eventHandlerTypeName);
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
