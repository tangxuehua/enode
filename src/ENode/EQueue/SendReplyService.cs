using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Serializing;
using ECommon.Utilities;

namespace ENode.EQueue
{
    internal class SendReplyService
    {
        private readonly ConcurrentDictionary<string, SocketRemotingClient> _remotingClientDict;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        public SendReplyService()
        {
            _remotingClientDict = new ConcurrentDictionary<string, SocketRemotingClient>();
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public void Stop()
        {
            foreach (var remotingClient in _remotingClientDict.Values)
            {
                remotingClient.Shutdown();
            }
        }
        public void SendReply(short replyType, object replyData, string replyAddress)
        {
            Task.Factory.StartNew(obj =>
            {
                var context = obj as SendReplyContext;
                try
                {
                    var remotingClient = GetRemotingClient(context.ReplyAddress);
                    if (remotingClient == null) return;

                    if (!remotingClient.IsConnected)
                    {
                        _logger.Error("Send command reply failed as remotingClient is not connected, replyAddress: " + context.ReplyAddress);
                        return;
                    }

                    var message = _jsonSerializer.Serialize(context.ReplyData);
                    var body = Encoding.UTF8.GetBytes(message);
                    var request = new RemotingRequest(context.ReplyType, body);

                    remotingClient.InvokeOneway(request);
                }
                catch (Exception ex)
                {
                    _logger.Error("Send command reply has exeption, replyAddress: " + context.ReplyAddress, ex);
                }
            }, new SendReplyContext(replyType, replyData, replyAddress));
        }

        private SocketRemotingClient GetRemotingClient(string replyAddress)
        {
            SocketRemotingClient remotingClient;
            if (_remotingClientDict.TryGetValue(replyAddress, out remotingClient))
            {
                return remotingClient;
            }

            var replyEndpoint = TryParseReplyAddress(replyAddress);
            if (replyEndpoint == null) return null;

            _ioHelper.TryIOAction("CreateReplyRemotingClient", () => "replyAddress:" + replyAddress, () => CreateReplyRemotingClient(replyEndpoint), 3);

            return remotingClient;
        }
        private IPEndPoint TryParseReplyAddress(string replyAddress)
        {
            try
            {
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
        private SocketRemotingClient CreateReplyRemotingClient(IPEndPoint replyEndpoint)
        {
            return _remotingClientDict.GetOrAdd(replyEndpoint.ToString(), key =>
            {
                return new SocketRemotingClient(replyEndpoint).Start();
            });
        }

        class SendReplyContext
        {
            public short ReplyType { get; private set; }
            public object ReplyData { get; private set; }
            public string ReplyAddress { get; private set; }

            public SendReplyContext(short replyType, object replyData, string replyAddress)
            {
                ReplyType = replyType;
                ReplyData = replyData;
                ReplyAddress = replyAddress;
            }
        }
    }
}
