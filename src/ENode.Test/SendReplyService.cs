using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Extensions;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    internal class SendReplyService : ISocketClientEventListener
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ConcurrentDictionary<string, SocketRemotingClient> _sendReplyRemotingClientDict;
        private readonly ConcurrentDictionary<string, ManualResetEvent> _remotingClientWaitHandleDict;
        private readonly ILogger _logger;

        public SendReplyService()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _sendReplyRemotingClientDict = new ConcurrentDictionary<string, SocketRemotingClient>();
            _remotingClientWaitHandleDict = new ConcurrentDictionary<string, ManualResetEvent>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public void SendCommandReplyMessageRequestAsync(CommandExecutedMessage message, string replyAddress)
        {

        }

        private async void SendRemotingRequestAsync(CommandExecutedMessage message, string replyAddress)
        {
            var replyEndpoint = TryParseReplyAddress(replyAddress);
            if (replyEndpoint == null) return;

            var json = _jsonSerializer.Serialize(message);
            var data = Encoding.UTF8.GetBytes(json);
            try
            {
                var remotingClient = CreateReplyRemotingClient(replyEndpoint);
                var remotingRequest = new RemotingRequest(Constants.SendCommandReplyMessageRequestCode, data);
                var remotingResponse = await remotingClient.InvokeAsync(remotingRequest, 5000);
                if (remotingResponse.Code != Constants.SuccessResponseCode)
                {
                    _logger.ErrorFormat("Send command executed message failed. replyAddress: {0}, message: {1}, responseCode: {2}", replyAddress, messageJson, remotingResponse.Code);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Send command executed message failed. replyAddress: {0}, message: {1}", replyAddress, messageJson), ex);
            }
        }
        private IPEndPoint TryParseReplyAddress(string replyAddress)
        {
            try
            {
                Ensure.NotNull(replyAddress, "replyAddress");
                var items = replyAddress.Split(':');
                Ensure.Equals(items.Length, 2);
                return new IPEndPoint(IPAddress.Parse(items[0]), int.Parse(items[1]));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Invalid reply address : {0}", replyAddress), ex);
                return null;
            }
        }
        private SocketRemotingClient CreateReplyRemotingClient(string replyAddress, IPEndPoint replyEndpoint)
        {
            ManualResetEvent waitHandle = null;
            var client = _sendReplyRemotingClientDict.GetOrAdd(replyAddress, key =>
            {
                waitHandle = new ManualResetEvent(false);
                if (_remotingClientWaitHandleDict.TryAdd(key, waitHandle))
                {

                }
                return new SocketRemotingClient(null, replyEndpoint);
            });

            if (waitHandle != null)
            {
                waitHandle.WaitOne();
            }
        }
    }
}
