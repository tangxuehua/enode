using System;
using System.Collections.Generic;
using System.Text;
using ECommon.IoC;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Scheduling;
using ECommon.Serializing;
using ENode.Distribute.EventStore.Protocols;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Distribute.EventStore
{
    public class RemotingEventStore : IEventStore, IEventStoreClient
    {
        private readonly byte[] EmptyData = new byte[0];
        private readonly SocketRemotingClient _remotingClient;
        private readonly IBinarySerializer _binarySerializer;
        private readonly List<int> _taskIds = new List<int>();
        private readonly IScheduleService _scheduleService;
        private readonly ILogger _logger;
        private bool _isServerAvailable;

        public EventStoreClientSetting Setting { get; private set; }
        public bool IsAvailable
        {
            get { return _isServerAvailable; }
        }

        public RemotingEventStore() : this(null) { }
        public RemotingEventStore(EventStoreClientSetting setting)
        {
            Setting = setting ?? new EventStoreClientSetting();
            _remotingClient = new SocketRemotingClient(Setting.ServerAddress, Setting.ServerPort);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
        }

        public void Initialize() { }
        public void Start()
        {
            _remotingClient.Start();
            CheckEventStoreServerAvailable();
            AssertServerAvailable();
            _taskIds.Add(_scheduleService.ScheduleTask(CheckEventStoreServerAvailable, Setting.CheckAvailableInterval, Setting.CheckAvailableInterval));
            _logger.InfoFormat("EventStore client started.");
        }
        public void Shutdown()
        {
            _remotingClient.Shutdown();
            foreach (var taskId in _taskIds)
            {
                _scheduleService.ShutdownTask(taskId);
            }
            _logger.InfoFormat("EventStore client shutdown.");
        }

        public EventByteStream GetEventStream(string aggregateRootId, Guid commandId)
        {
            AssertServerAvailable();
            var data = _binarySerializer.Serialize(new GetEventStreamRequest(aggregateRootId, commandId));
            var remotingRequest = new RemotingRequest((int)RequestCode.GetEventStream, data);
            var remotingResponse = _remotingClient.InvokeSync(remotingRequest, 10000);
            if (remotingResponse.Code == (int)ResponseCode.Success)
            {
                return _binarySerializer.Deserialize<EventByteStream>(remotingResponse.Body);
            }
            else
            {
                var errorMessage = Encoding.UTF8.GetString(remotingResponse.Body);
                throw new ENodeException(string.Format("Get event stream from remoting event store server failed. errorMessage:{0}", errorMessage));
            }
        }
        public EventCommitStatus Commit(EventByteStream stream)
        {
            AssertServerAvailable();
            var data = _binarySerializer.Serialize(stream);
            var remotingRequest = new RemotingRequest((int)RequestCode.StoreEvent, data);
            var remotingResponse = _remotingClient.InvokeSync(remotingRequest, 10000);
            if (remotingResponse.Code == (int)ResponseCode.Success)
            {
                return (EventCommitStatus)BitConverter.ToInt32(remotingResponse.Body, 0);
            }
            else
            {
                var errorMessage = Encoding.UTF8.GetString(remotingResponse.Body);
                throw new ENodeException(string.Format("Commit event stream to remoting event store server failed. errorMessage:{0}", errorMessage));
            }
        }
        public IEnumerable<EventByteStream> Query(string aggregateRootId, string aggregateRootName, long minStreamVersion, long maxStreamVersion)
        {
            AssertServerAvailable();
            var data = _binarySerializer.Serialize(new QueryAggregateEventStreamsRequest(aggregateRootId, aggregateRootName, minStreamVersion, maxStreamVersion));
            var remotingRequest = new RemotingRequest((int)RequestCode.GetEventStream, data);
            var remotingResponse = _remotingClient.InvokeSync(remotingRequest, 10000);
            if (remotingResponse.Code == (int)ResponseCode.Success)
            {
                return _binarySerializer.Deserialize<IEnumerable<EventByteStream>>(remotingResponse.Body);
            }
            else
            {
                var errorMessage = Encoding.UTF8.GetString(remotingResponse.Body);
                throw new ENodeException(string.Format("Query aggregate event streams from remoting event store server failed. errorMessage:{0}", errorMessage));
            }
        }
        public IEnumerable<EventByteStream> QueryAll()
        {
            AssertServerAvailable();
            //TODO
            return new List<EventByteStream>();
        }

        private void CheckEventStoreServerAvailable()
        {
            var remotingRequest = new RemotingRequest((int)RequestCode.DetectAlive, EmptyData);
            var remotingResponse = _remotingClient.InvokeSync(remotingRequest, 10000);
            if (remotingResponse.Code == (int)ResponseCode.Success)
            {
                var result = BitConverter.ToInt32(remotingResponse.Body, 0);
                _isServerAvailable = result == 1;
            }
            else
            {
                _isServerAvailable = false;
                _logger.ErrorFormat("Detect event store server available meet exception, remoting response code:{0}, errorMessage:{1}", remotingResponse.Code, Encoding.UTF8.GetString(remotingResponse.Body));
            }
        }
        private void AssertServerAvailable()
        {
            if (!_isServerAvailable)
            {
                throw new ENodeException("EventStore server not available.");
            }
        }
    }
}
