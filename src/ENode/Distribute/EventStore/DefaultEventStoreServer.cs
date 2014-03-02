using System;
using System.Net.Sockets;
using ECommon.IoC;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Socketing;
using ENode.Distribute.EventStore.Protocols;
using ENode.Distribute.EventStore.RequestHandlers;

namespace ENode.Distribute.EventStore
{
    public class DefaultEventStoreServer : IEventStoreServer
    {
        private readonly ILogger _logger;
        private readonly SocketRemotingServer _remotingServer;

        public SocketSetting Setting { get; set; }

        public DefaultEventStoreServer() : this(null) { }
        public DefaultEventStoreServer(SocketSetting setting)
        {
            Setting = setting ?? new SocketSetting { Address = SocketUtils.GetLocalIPV4().ToString(), Port = 10000, Backlog = 10000 };
            _remotingServer = new SocketRemotingServer("EventStoreRemotingServer", Setting, new EventStoreClientSocketEventListener(this));
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
        }

        public DefaultEventStoreServer Initialize()
        {
            _remotingServer.RegisterRequestHandler((int)RequestCode.DetectAlive, new DetectAliveRequestHandler());
            _remotingServer.RegisterRequestHandler((int)RequestCode.StoreEvent, new StoreEventRequestHandler());
            _remotingServer.RegisterRequestHandler((int)RequestCode.GetEventStream, new GetEventStreamRequestHandler());
            _remotingServer.RegisterRequestHandler((int)RequestCode.QueryAggregateEventStreams, new QueryAggregateEventStreamsRequestHandler());
            return this;
        }

        public void Start()
        {
            _remotingServer.Start();
            _logger.InfoFormat("EventStore server started, listening address:[{0}:{1}]", Setting.Address, Setting.Port);
        }
        public void Shutdown()
        {
            _remotingServer.Shutdown();
            _logger.InfoFormat("EventStore server shutdown.");
        }

        class EventStoreClientSocketEventListener : ISocketEventListener
        {
            private DefaultEventStoreServer _eventStoreServer;

            public EventStoreClientSocketEventListener(DefaultEventStoreServer eventStoreServer)
            {
                _eventStoreServer = eventStoreServer;
            }

            public void OnNewSocketAccepted(SocketInfo socketInfo)
            {
                _eventStoreServer._logger.InfoFormat("Accepted new event store client, address:{0}", socketInfo.SocketRemotingEndpointAddress);
            }

            public void OnSocketReceiveException(SocketInfo socketInfo, Exception exception)
            {
                var socketException = exception as SocketException;
                if (socketException != null)
                {
                    _eventStoreServer._logger.InfoFormat("Event store client SocketException, address:{0}, errorCode:{1}", socketInfo.SocketRemotingEndpointAddress, socketException.SocketErrorCode);
                }
                else
                {
                    _eventStoreServer._logger.InfoFormat("Event store client Exception, address:{0}, errorMsg:", socketInfo.SocketRemotingEndpointAddress, exception.Message);
                }
            }
        }
    }
}
