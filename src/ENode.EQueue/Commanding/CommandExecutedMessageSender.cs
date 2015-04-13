using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class CommandExecutedMessageSender
    {
        private const string DefaultCommandExecutedMessageSenderProcuderId = "CommandExecutedMessageSender";
        private readonly Producer _producer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly SendReplyService _sendReplyService;
        private readonly ILogger _logger;

        public Producer Producer { get { return _producer; } }

        public CommandExecutedMessageSender(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultCommandExecutedMessageSenderProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _sendReplyService = ObjectContainer.Resolve<SendReplyService>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public CommandExecutedMessageSender Start()
        {
            _producer.Start();
            return this;
        }
        public CommandExecutedMessageSender Shutdown()
        {
            _producer.Shutdown();
            return this;
        }
        public async void SendAsync(CommandExecutedMessage message, string replyAddress)
        {
            var messageJson = _jsonSerializer.Serialize(message);
            var data = Encoding.UTF8.GetBytes(messageJson);
            try
            {
                var remotingClient = CreateReplyRemotingClient(replyAddress);
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

        private SocketRemotingClient CreateReplyRemotingClient(string replyAddress)
        {
            return _sendReplyRemotingClientDict.GetOrAdd(replyAddress, key =>
            {
                Ensure.NotNull(key, "replyAddress");
                var items = key.Split(':');
                Ensure.Equals(items.Length, 2);
                var remotingClient = new SocketRemotingClient(new IPEndPoint(IPAddress.Parse(items[0]), int.Parse(items[1])));
                remotingClient.Start();
                return remotingClient;
            });
        }
        private void SendMessageAsync(EQueueMessage message, string messageJson, string routingKey, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("PublishQueueMessageAsync",
            () => _sendMessageService.SendMessageAsync(_producer, message, routingKey),
            currentRetryTimes => SendMessageAsync(message, messageJson, routingKey, currentRetryTimes),
            null,
            () => string.Format("[message:{0}]", messageJson),
            null,
            retryTimes);
        }
    }
}
