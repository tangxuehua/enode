using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly string _name;
        private readonly ConcurrentDictionary<string, SocketRemotingClientWrapper> _remotingClientDict;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IScheduleService _scheduleService;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly string _scanInactiveRemotingClientTaskName;

        public SendReplyService(string name)
        {
            _name = name;
            _remotingClientDict = new ConcurrentDictionary<string, SocketRemotingClientWrapper>();
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _scheduleService = ObjectContainer.Resolve<IScheduleService>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _scanInactiveRemotingClientTaskName = name + "_ScanInactiveRemotingClient_" + DateTime.Now.Ticks + new Random().Next(10000);
        }

        public void Start()
        {
            _scheduleService.StartTask(_scanInactiveRemotingClientTaskName, ScanInactiveRemotingClients, 5000, 5000);
        }
        public void Stop()
        {
            _scheduleService.StopTask(_scanInactiveRemotingClientTaskName);
            foreach (var remotingClient in _remotingClientDict.Values)
            {
                remotingClient.SocketRemotingClient.Shutdown();
            }
        }
        public Task SendReply(short replyType, object replyData, string replyAddress)
        {
            Task.Factory.StartNew(obj =>
            {
                var context = obj as SendReplyContext;
                try
                {
                    var message = _jsonSerializer.Serialize(context.ReplyData);
                    var body = Encoding.UTF8.GetBytes(message);
                    var request = new RemotingRequest(context.ReplyType, body);
                    var remotingClientWrapper = GetRemotingClient(context.ReplyAddress);
                    remotingClientWrapper.SocketRemotingClient.InvokeOneway(request);
                    remotingClientWrapper.LastSendMessageTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.Error("Send reply has exeption, replyAddress: " + context.ReplyAddress, ex);
                }
            }, new SendReplyContext(replyType, replyData, replyAddress));
            return Task.CompletedTask;
        }

        private void ScanInactiveRemotingClients()
        {
            lock (this)
            {
                var inactiveList = new List<KeyValuePair<string, SocketRemotingClientWrapper>>();
                foreach (var pair in _remotingClientDict)
                {
                    if (!pair.Value.SocketRemotingClient.IsConnected || (DateTime.Now - pair.Value.LastSendMessageTime).TotalSeconds > 300)
                    {
                        inactiveList.Add(pair);
                    }
                }
                foreach (var pair in inactiveList)
                {
                    if (_remotingClientDict.TryRemove(pair.Key, out SocketRemotingClientWrapper removed))
                    {
                        removed.SocketRemotingClient.Shutdown();
                        _logger.InfoFormat("Removed disconnected remoting client, remotingAddress: {0}", pair.Key);
                    }
                }
            }
        }
        private SocketRemotingClientWrapper GetRemotingClient(string replyAddress)
        {
            lock (this)
            {
                if (_remotingClientDict.TryGetValue(replyAddress, out SocketRemotingClientWrapper remotingClientWrapper))
                {
                    if (remotingClientWrapper.SocketRemotingClient.IsConnected)
                    {
                        return remotingClientWrapper;
                    }
                    else
                    {
                        _remotingClientDict.TryRemove(replyAddress, out SocketRemotingClientWrapper removed);
                    }
                }

                return _ioHelper.TryIOFunc("CreateReplyRemotingClient", () => "replyAddress:" + replyAddress, () => CreateReplyRemotingClient(replyAddress), 3);
            }
        }
        private SocketRemotingClientWrapper CreateReplyRemotingClient(string replyAddress)
        {
            return _remotingClientDict.GetOrAdd(replyAddress, key =>
            {
                return new SocketRemotingClientWrapper
                {
                    SocketRemotingClient = new SocketRemotingClient(_name, TryParseReplyAddress(replyAddress)).Start(),
                    LastSendMessageTime = DateTime.Now
                };
            });
        }
        private string ToReplyAddress(IPEndPoint replyEndpoint)
        {
            return string.Format("{0}:{1}", replyEndpoint.Address.ToString(), replyEndpoint.Port);
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
            public SocketRemotingClient SocketRemotingClient { get; set; }
            public DateTime LastSendMessageTime { get; set; }
        }
    }
}
