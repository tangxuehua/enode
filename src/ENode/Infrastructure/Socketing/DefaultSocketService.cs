using System;
using System.Net.Sockets;
using System.Text;
using ENode.Infrastructure.Logging;

namespace ENode.Infrastructure.Socketing
{
    public class DefaultSocketService : ISocketService
    {
        private ILogger _logger;

        public DefaultSocketService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType().Name);
        }
        public void SendMessage(Socket targetSocket, byte[] message, Action<byte[]> messageSentCallback)
        {
            if (message.Length > 0)
            {
                var context = new SendContext { TargetSocket = targetSocket, Message = message, MessageSentCallback = messageSentCallback };
                targetSocket.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendCallback), context);
            }
        }
        public void ReceiveMessage(Socket sourceSocket, Action<byte[]> messageReceivedCallback)
        {
            ReceiveInternal(new ReceiveState
            {
                SourceSocket = sourceSocket,
                MessageReceivedCallback = messageReceivedCallback
            }, 4);
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var context = (SendContext)asyncResult.AsyncState;
                context.TargetSocket.EndSend(asyncResult);
                context.MessageSentCallback(context.Message);
            }
            catch (SocketException socketException)
            {
                _logger.ErrorFormat("Socket send exception, ErrorCode:{0}", socketException.SocketErrorCode);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Unknown socket send exception:{0}", ex);
            }
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            var receiveState = (ReceiveState)asyncResult.AsyncState;
            var sourceSocket = receiveState.SourceSocket;
            var data = receiveState.Data;
            var bytesRead = 0;

            try
            {
                bytesRead = sourceSocket.EndReceive(asyncResult);
            }
            catch (SocketException socketException)
            {
                _logger.ErrorFormat("Socket receive exception, ErrorCode:{0}", socketException.SocketErrorCode);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Unknown socket receive exception:{0}", ex);
            }

            if (bytesRead > 0)
            {
                if (receiveState.MessageSize == null)
                {
                    receiveState.MessageSize = SocketUtils.ParseMessageLength(receiveState.Buffer);
                    var size = receiveState.MessageSize <= ReceiveState.BufferSize ? receiveState.MessageSize.Value : ReceiveState.BufferSize;
                    ReceiveInternal(receiveState, size);
                }
                else
                {
                    for (var index = 0; index < bytesRead; index++)
                    {
                        data.Add(receiveState.Buffer[index]);
                    }
                    if (receiveState.Data.Count < receiveState.MessageSize.Value)
                    {
                        var remainSize = receiveState.MessageSize.Value - receiveState.Data.Count;
                        var size = remainSize <= ReceiveState.BufferSize ? remainSize : ReceiveState.BufferSize;
                        ReceiveInternal(receiveState, size);
                    }
                    else
                    {
                        receiveState.MessageReceivedCallback(data.ToArray());
                        receiveState.MessageSize = null;
                        receiveState.Data.Clear();
                        ReceiveInternal(receiveState, 4);
                    }
                }
            }
        }

        private IAsyncResult ReceiveInternal(ReceiveState receiveState, int size)
        {
            return receiveState.SourceSocket.BeginReceive(receiveState.Buffer, 0, size, 0, ReceiveCallback, receiveState);
        }
    }
}
