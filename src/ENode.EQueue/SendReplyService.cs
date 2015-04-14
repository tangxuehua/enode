using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Scheduling;
using ECommon.Serializing;
using ECommon.Utilities;

namespace ENode.EQueue
{
    internal class SendReplyService
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IScheduleService _scheduleService;
        private readonly ConcurrentDictionary<string, SocketRemotingClientWrapper> _sendReplyRemotingClientDict;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly IList<int> _taskIds;
        private const int MaxNotActiveTimeSeconds = 60 * 10;
        private const int ScanNotActiveClientInterval = 5000;

        public SendReplyService()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _sendReplyRemotingClientDict = new ConcurrentDictionary<string, SocketRemotingClientWrapper>();
            _taskIds = new List<int>();
        }

        public void Start()
        {
            _taskIds.Add(_scheduleService.ScheduleTask("RemoveNotActiveRemotingClient", RemoveNotActiveRemotingClient, ScanNotActiveClientInterval, ScanNotActiveClientInterval));
        }
        public void Shutdown()
        {
            foreach (var taskId in _taskIds)
            {
                _scheduleService.ShutdownTask(taskId);
            }
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
            var notActiveClientWrappers = _sendReplyRemotingClientDict.Values.Where(x => x.IsNotActive(MaxNotActiveTimeSeconds));
            foreach (var clientWrapper in notActiveClientWrappers)
            {
                var clientAddress = clientWrapper.ReplyEndpoint.ToString();
                SocketRemotingClientWrapper removedClientWrapper;
                if (_sendReplyRemotingClientDict.TryRemove(clientAddress, out removedClientWrapper))
                {
                    removedClientWrapper.RemotingClient.Shutdown();
                    _logger.InfoFormat("Closed and removed not active remoting client: {0}, lastActiveTime: {1}", clientAddress, removedClientWrapper.LastActiveTime);
                }
            }
        }
        private SocketRemotingClientWrapper GetRemotingClientWrapper(string replyAddress)
        {
            SocketRemotingClientWrapper remotingClientWrapper;
            if (_sendReplyRemotingClientDict.TryGetValue(replyAddress, out remotingClientWrapper))
            {
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
                var remotingClient = new SocketRemotingClient(replyEndpoint).Start();
                return new SocketRemotingClientWrapper
                {
                    ReplyEndpoint = replyEndpoint,
                    RemotingClient = remotingClient,
                    LastActiveTime = DateTime.Now
                };
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
