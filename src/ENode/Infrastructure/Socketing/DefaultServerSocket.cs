using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ENode.Infrastructure.Logging;

namespace ENode.Infrastructure.Socketing
{
    public class DefaultServerSocket : IServerSocket
    {
        private Socket _innerSocket;
        private Action<ReceiveContext> _messageReceivedCallback;
        private ManualResetEvent _newClientSocketSignal;
        private ISocketService _socketService;
        private ILogger _logger;

        public DefaultServerSocket(ISocketService socketService, ILoggerFactory loggerFactory)
        {
            _innerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _newClientSocketSignal = new ManualResetEvent(false);
            _socketService = socketService;
            _logger = loggerFactory.Create(GetType().Name);
        }

        public IServerSocket Listen(int backlog)
        {
            _innerSocket.Listen(backlog);
            return this;
        }
        public IServerSocket Bind(string address, int port)
        {
            _innerSocket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            return this;
        }
        public IClientSocket Start(Action<ReceiveContext> messageReceivedCallback)
        {
            _messageReceivedCallback = messageReceivedCallback;

            _logger.DebugFormat("socket is listening address:{0}", _innerSocket.LocalEndPoint.ToString());

            while (true)
            {
                _newClientSocketSignal.Reset();

                try
                {
                    _innerSocket.BeginAccept((asyncResult) =>
                    {
                        var clientSocket = _innerSocket.EndAccept(asyncResult);
                        _logger.DebugFormat("----accepted new client.");
                        _newClientSocketSignal.Set();
                        _socketService.ReceiveMessage(clientSocket, (message) =>
                        {
                            var receiveContext = new ReceiveContext
                            {
                                TargetSocket = clientSocket,
                                Message = message,
                                MessageProcessedCallback = (context) =>
                                {
                                    _socketService.SendMessage(context.TargetSocket, context.ReplyMessage, (reply) => { });
                                }
                            };
                            _messageReceivedCallback(receiveContext);
                            
                        });
                    }, _innerSocket);
                }
                catch (SocketException socketException)
                {
                    _logger.ErrorFormat("Socket exception, ErrorCode:{0}", socketException.SocketErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.ErrorFormat("Unknown socket exception:{0}", ex);
                }

                _newClientSocketSignal.WaitOne();
            }
        }
    }
}
