using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Scheduling;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.EQueue.Utils;

namespace ENode.EQueue
{
    internal class SendReplyService
    {
        private readonly IOHelper _ioHelper;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SocketRemotingClient> _remotingClientDict;
        private readonly string _scanInactiveCommandRemotingClientTaskName;
        private readonly IScheduleService _scheduleService;

        public SendReplyService()
        {
            _remotingClientDict = new ConcurrentDictionary<string, SocketRemotingClient>();
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _scanInactiveCommandRemotingClientTaskName = "ScanInactiveCommandRemotingClient_" + DateTime.Now.Ticks + new Random().Next(10000);
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

        public void Start()
        {
            _scheduleService.StartTask(_scanInactiveCommandRemotingClientTaskName, ScanInactiveRemotingClients, 5000, 5000);
        }

        public void Stop()
        {
            _scheduleService.StopTask(_scanInactiveCommandRemotingClientTaskName);
            foreach (var remotingClient in _remotingClientDict.Values)
            {
                remotingClient.Shutdown();
            }
        }

        private SocketRemotingClient CreateReplyRemotingClient(string replyAddress, IPEndPoint replyEndpoint)
        {
            return _remotingClientDict.GetOrAdd(replyAddress, key =>
            {
                return new SocketRemotingClient(replyEndpoint).Start();
            });
        }

        private SocketRemotingClient GetRemotingClient(string replyAddress)
        {
            var replyEndpoint = TryParseReplyAddress(replyAddress);
            if (replyEndpoint == null) return null;

            SocketRemotingClient remotingClient;
            if (_remotingClientDict.TryGetValue(replyAddress, out remotingClient))
            {
                return remotingClient;
            }

            _ioHelper.TryIOAction("CreateReplyRemotingClient", () => "replyAddress:" + replyAddress, () => CreateReplyRemotingClient(replyAddress, replyEndpoint), 3);

            if (_remotingClientDict.TryGetValue(replyAddress, out remotingClient))
            {
                return remotingClient;
            }

            return null;
        }

        private void ScanInactiveRemotingClients()
        {
            var inactiveList = new List<KeyValuePair<string, SocketRemotingClient>>();
            foreach (var pair in _remotingClientDict)
            {
                if (!pair.Value.IsConnected)
                {
                    inactiveList.Add(pair);
                }
            }
            foreach (var pair in inactiveList)
            {
                SocketRemotingClient removed;
                if (_remotingClientDict.TryRemove(pair.Key, out removed))
                {
                    _logger.InfoFormat("Removed disconnected command remoting client, remotingAddress: {0}", pair.Key);
                }
            }
        }

        private IPEndPoint TryParseReplyAddress(string replyAddress)
        {
            try
            {
                var items = replyAddress.Split(':');
                Ensure.Equals(items.Length, 2);
                var hostNameType = Uri.CheckHostName(items[0]);

                IPEndPoint ipEndPoint;
                switch (hostNameType)
                {
                    case UriHostNameType.Dns:
                        ipEndPoint = SocketUtils.GetIPEndPointFromHostName(items[0], int.Parse(items[1]), AddressFamily.InterNetwork, false);
                        break;

                    case UriHostNameType.IPv4:
                    case UriHostNameType.IPv6:
                        ipEndPoint = new IPEndPoint(IPAddress.Parse(items[0]), int.Parse(items[1]));
                        break;

                    case UriHostNameType.Unknown:
                    default:
                        throw new Exception($"Host name type[{hostNameType}] can not resolve.");
                }

                return new IPEndPoint(IPAddress.Parse(items[0]), int.Parse(items[1]));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Invalid reply address : {0}", replyAddress), ex);
                return null;
            }
        }

        private class SendReplyContext
        {
            public SendReplyContext(short replyType, object replyData, string replyAddress)
            {
                ReplyType = replyType;
                ReplyData = replyData;
                ReplyAddress = replyAddress;
            }

            public string ReplyAddress { get; private set; }
            public object ReplyData { get; private set; }
            public short ReplyType { get; private set; }
        }
    }
}