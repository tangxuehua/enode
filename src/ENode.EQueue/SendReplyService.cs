using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Retring;
using ECommon.Scheduling;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;

namespace ENode.EQueue
{
    public class SendReplyService : ISocketClientEventListener
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IScheduleService _scheduleService;
        private readonly ConcurrentDictionary<string, SocketRemotingClientWrapper> _sendReplyRemotingClientDict;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private const int MaxNotActiveTimeSeconds = 60 * 10;

        public SendReplyService(IOHelper ioHelper, ILoggerFactory loggerFactory)
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _sendReplyRemotingClientDict = new ConcurrentDictionary<string, SocketRemotingClientWrapper>();
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
            _scheduleService.ScheduleTask("RemoveNotActiveRemotingClient", RemoveNotActiveRemotingClient, 5000, 5000);
        }

        public void SendReply(int replyType, object replyData, string replyAddress)
        {
            Ensure.Positive(replyType, "replyType");
            Ensure.NotNull(replyData, "replyData");
            Ensure.NotNull(replyAddress, "replyAddress");

            var remotingClientWrapper = GetRemotingClientWrapper(replyAddress);
            if (remotingClientWrapper == null) return;

            var message = _jsonSerializer.Serialize(replyData);
            var body = Encoding.UTF8.GetBytes(message);
            var remotingRequest = new RemotingRequest(replyType, body);

            _ioHelper.TryIOAction("SendReplyAsync", () => string.Format("[replyAddress: {0}, replyType: {1}, message: {2}]", replyAddress, replyType, message), () =>
            {
                _ioHelper.TryIOAction(async () =>
                {
                    var remotingResponse = await remotingClientWrapper.RemotingClient.InvokeAsync(remotingRequest);
                    if (remotingResponse.Code != Constants.SuccessResponseCode)
                    {
                        throw new IOException("Send reply remoting request failed.");
                    }
                    remotingClientWrapper.LastActiveTime = DateTime.Now;
                }, "SendReplyRemotingRequestAsync");
            }, 3);
        }

        private void RemoveNotActiveRemotingClient()
        {
            foreach (var clientWrapper in _sendReplyRemotingClientDict.Values)
            {
                if (clientWrapper.IsNotActive(MaxNotActiveTimeSeconds))
                {
                    _logger.InfoFormat("Removed not active remoting client: {0}", clientWrapper.ReplyEndpoint.ToString());
                }
            }
        }
        private SocketRemotingClientWrapper GetRemotingClientWrapper(string replyAddress)
        {
            SocketRemotingClientWrapper remotingClientWrapper;
            if (_sendReplyRemotingClientDict.TryGetValue(replyAddress, out remotingClientWrapper))
            {
                //TODO, check if the remoting client is closed.
                return remotingClientWrapper;
            }
            var replyEndpoint = TryParseReplyAddress(replyAddress);
            if (replyEndpoint == null) return null;

            _ioHelper.TryIOAction("CreateReplyRemotingClient", () => "replyAddress:" + replyAddress, () =>
            {
                remotingClientWrapper = CreateReplyRemotingClient(replyEndpoint);
            }, 3);

            return remotingClientWrapper;
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
        private SocketRemotingClientWrapper CreateReplyRemotingClient(IPEndPoint replyEndpoint)
        {
            return _sendReplyRemotingClientDict.GetOrAdd(replyEndpoint.ToString(), key =>
            {
                var remotingClient = new SocketRemotingClient(replyEndpoint);
                remotingClient.Start();
                return new SocketRemotingClientWrapper { ReplyEndpoint = replyEndpoint, RemotingClient = remotingClient, LastActiveTime = DateTime.Now };
            });
        }

        class SocketRemotingClientWrapper
        {
            public IPEndPoint ReplyEndpoint { get; set; }
            public SocketRemotingClient RemotingClient { get; set; }
            public DateTime LastActiveTime { get; set; }

            public bool IsNotActive(int maxNotActiveSeconds)
            {
                return (DateTime.Now - LastActiveTime).TotalSeconds >= maxNotActiveSeconds;
            }
        }
    }
}
