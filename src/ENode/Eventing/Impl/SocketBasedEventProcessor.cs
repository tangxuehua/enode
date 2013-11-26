using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ENode.Infrastructure;
using ENode.Infrastructure.Concurrent;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Retring;
using ENode.Infrastructure.Serializing;
using ENode.Infrastructure.Socketing;

namespace ENode.Eventing.Impl
{
    /// <summary>The socket-based implementation of IEventProcessor.
    /// </summary>
    public class SocketBasedEventProcessor : IEventProcessor
    {
        #region Private Variables

        private readonly IList<Worker> _workers;
        private readonly IUncommittedEventSender _uncommittedEventSender;
        private readonly IServerSocket _serverSocket;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventStore _eventStore;
        private readonly ICommittedEventSender _committedEventSender;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly IEventSynchronizerProvider _eventSynchronizerProvider;
        private readonly ILogger _logger;
        private bool _started;

        #endregion

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="backlog"></param>
        /// <param name="workerCount"></param>
        public SocketBasedEventProcessor(string address = "127.0.0.1", int port = 5000, int backlog = 5000, int workerCount = 1)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(address);
            }
            if (port <= 0)
            {
                throw new Exception(string.Format("Invalid socket port:{0}, socket port cannot be negative.", port));
            }
            if (backlog <= 0)
            {
                throw new Exception(string.Format("Invalid socket backlog:{0}, socket backlog cannot be negative.", backlog));
            }
            if (workerCount <= 0)
            {
                throw new Exception(string.Format("There must at least one worker for {0}.", GetType().Name));
            }

            _workers = new List<Worker>();
            _uncommittedEventSender = ObjectContainer.Resolve<IUncommittedEventSender>();
            _serverSocket = ObjectContainer.Resolve<IServerSocket>();
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _eventStore = ObjectContainer.Resolve<IEventStore>();
            _committedEventSender = ObjectContainer.Resolve<ICommittedEventSender>();
            _actionExecutionService = ObjectContainer.Resolve<IActionExecutionService>();
            _eventSynchronizerProvider = ObjectContainer.Resolve<IEventSynchronizerProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);

            _serverSocket.Bind(address, port).Listen(backlog);

            for (var index = 0; index < workerCount; index++)
            {
                _workers.Add(new Worker(HandleEventStream));
            }

            _started = false;
        }

        public void Initialize() { }

        public void Start()
        {
            if (_started) return;

            _serverSocket.Start((receiveContext) => _uncommittedEventSender.Send(receiveContext));

            _started = true;
            _logger.InfoFormat("{0} started...", this.GetType().Name);
        }

        #region Private Methods

        private void HandleEventStream()
        {
            var receiveContext = _queue.Take();

            var context = new EventProcessingContext { EventStream = ParseEventData(receiveContext.Message) };

            try
            {
                CommitEvents(context);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when committing eventStream:{0}.", context.EventStream.GetStreamInformation()), ex);
            }

            var result = new EventProcessResult { EventStreamId = context.EventStream.Id, Status = context.ProcessStatus };
            receiveContext.ReplyMessage = _binarySerializer.Serialize(result);
            receiveContext.MessageProcessedCallback(receiveContext);
        }
        private EventStream ParseEventData(byte[] eventData)
        {
            return _binarySerializer.Deserialize<EventStream>(eventData);
        }
        private void CommitEvents(EventProcessingContext context)
        {
            SyncBeforeEventPersisting(context);
            if (context.ProcessStatus != EventProcessStatus.Success)
            {
                return;
            }

            PersistEvents(context);
            if (context.ProcessStatus != EventProcessStatus.Success)
            {
                return;
            }

            SyncAfterEventPersisted(context);
            PublishEvents(context);
        }
        private void PersistEvents(EventProcessingContext context)
        {
            var eventStream = context.EventStream;

            Func<bool> persistEvents = () =>
            {
                try
                {
                    _eventStore.Append(eventStream);
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format("{0} raised when persisting events:{1}", ex.GetType().Name, eventStream.GetStreamInformation());
                    _logger.Error(errorMessage, ex);

                    if (ex is ConcurrentException)
                    {
                        context.ProcessStatus = EventProcessStatus.ConcurrentException;
                    }
                    else
                    {
                        context.ProcessStatus = EventProcessStatus.Failed;
                    }
                }

                return context.ProcessStatus == EventProcessStatus.Success;
            };

            _actionExecutionService.TryRecursively("PersistEvents", persistEvents, 3);
        }
        private void PublishEvents(EventProcessingContext context)
        {
            var eventStream = context.EventStream;

            Func<bool> publishEvents = () =>
            {
                try
                {
                    _committedEventSender.Send(eventStream);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when publishing events:{0}", eventStream.GetStreamInformation()), ex);
                    context.ProcessStatus = EventProcessStatus.PublishFailed;
                }

                return context.ProcessStatus == EventProcessStatus.Success;
            };

            _actionExecutionService.TryRecursively("PublishEvents", publishEvents, 3);
        }
        private void SyncBeforeEventPersisting(EventProcessingContext context)
        {
            Func<bool> syncBeforeEventPersisting = () =>
            {
                var synchronizeResult = SynchronizeResult.Success;

                foreach (var evnt in context.EventStream.Events)
                {
                    var synchronizers = _eventSynchronizerProvider.GetSynchronizers(evnt.GetType());
                    foreach (var synchronizer in synchronizers)
                    {
                        try
                        {
                            synchronizer.OnBeforePersisting(evnt);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format(
                                "Exception raised when calling synchronizer's OnBeforePersisting method. synchronizer:{0}, event:{1}",
                                synchronizer.GetInnerSynchronizer().GetType().Name,
                                evnt.GetType().Name), ex);

                            if (ex is ConcurrentException)
                            {
                                synchronizeResult = SynchronizeResult.SynchronizerConcurrentException;
                            }
                            else
                            {
                                synchronizeResult = SynchronizeResult.Failed;
                            }

                            break;
                        }
                    }
                }

                if (synchronizeResult == SynchronizeResult.Success)
                {
                    context.ProcessStatus = EventProcessStatus.Success;
                }
                else if (synchronizeResult == SynchronizeResult.Failed)
                {
                    context.ProcessStatus = EventProcessStatus.SynchronizerFailed;
                    //CompleteCommandTask(context.EventStream, synchronizeResult.Exception);
                    //CleanEvents(context);
                }
                else if (synchronizeResult == SynchronizeResult.SynchronizerConcurrentException)
                {
                    context.ProcessStatus = EventProcessStatus.SynchronizerConcurrentException;
                }

                return context.ProcessStatus == EventProcessStatus.Success || context.ProcessStatus == EventProcessStatus.SynchronizerFailed;
            };

            _actionExecutionService.TryRecursively("SyncBeforeEventPersisting", syncBeforeEventPersisting, 3);
        }
        private void SyncAfterEventPersisted(EventProcessingContext context)
        {
            foreach (var evnt in context.EventStream.Events)
            {
                var synchronizers = _eventSynchronizerProvider.GetSynchronizers(evnt.GetType());
                foreach (var synchronizer in synchronizers)
                {
                    try
                    {
                        synchronizer.OnAfterPersisted(evnt);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format(
                            "Exception raised when calling synchronizer's OnAfterPersisted method. synchronizer:{0}, event:{1}",
                            synchronizer.GetInnerSynchronizer().GetType().Name,
                            evnt.GetType().Name), ex);
                    }
                }
            }
        }

        #endregion

        enum SynchronizeResult
        {
            Success,
            SynchronizerConcurrentException,
            Failed
        }
    }
}
