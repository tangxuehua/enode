using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        private readonly ConcurrentDictionary<string, SocketRemotingClientWrapper> _clientWrapperDict;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IScheduleService _scheduleService;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private const int MaxNotActiveTimeSeconds = 60;
        private const int ScanNotActiveClientInterval = 5000;

        public SendReplyService()
        {
            _clientWrapperDict = new ConcurrentDictionary<string, SocketRemotingClientWrapper>();
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public void Start()
        {
            _scheduleService.StartTask("RemoveNotActiveRemotingClient", RemoveNotActiveRemotingClient, 1000, ScanNotActiveClientInterval);
        }
        public void Stop()
        {
            _scheduleService.StopTask("RemoveNotActiveRemotingClient");
        }
        public void SendReply(short replyType, object replyData, string replyAddress)
        {
            Task.Factory.StartNew(obj =>
            {
                var context = obj as SendReplyContext;
                try
                {
                    var clientWrapper = GetRemotingClientWrapper(context.ReplyAddress);
                    if (clientWrapper == null) return;

                    var message = _jsonSerializer.Serialize(context.ReplyData);
                    var body = Encoding.UTF8.GetBytes(message);
                    var request = new RemotingRequest(context.ReplyType, body);

                    clientWrapper.RemotingClient.InvokeOneway(request);
                }
                catch (Exception ex)
                {
                    _logger.Error("Send command reply failed, replyAddress: " + context.ReplyAddress, ex);
                }
            }, new SendReplyContext(replyType, replyData, replyAddress));
        }

        private void RemoveNotActiveRemotingClient()
        {
            var expiredEntries = _clientWrapperDict.Where(x => x.Value.IsNotActive(MaxNotActiveTimeSeconds));
            foreach (var entry in expiredEntries)
            {
                SocketRemotingClientWrapper clientWrapper;
                if (_clientWrapperDict.TryRemove(entry.Key, out clientWrapper))
                {
                    clientWrapper.RemotingClient.Shutdown();
                    _logger.InfoFormat("Closed and removed not active remoting client: {0}, lastActiveTime: {1}", entry.Key, clientWrapper.LastActiveTime);
                }
            }
        }
        private SocketRemotingClientWrapper GetRemotingClientWrapper(string replyAddress)
        {
            SocketRemotingClientWrapper remotingClientWrapper;
            if (_clientWrapperDict.TryGetValue(replyAddress, out remotingClientWrapper))
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
            return _clientWrapperDict.GetOrAdd(replyEndpoint.ToString(), key =>
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
