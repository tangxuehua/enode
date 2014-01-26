using System;
using System.Collections.Concurrent;
using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ENode.Commanding;
using ENode.Domain;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using EQueue.Utils;

namespace ENode.EQueue
{
    public class CommandConsumer : IMessageHandler
    {
        private readonly Consumer _consumer;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IRepository _repository;
        private readonly ConcurrentDictionary<Guid, IMessageContext> _messageContextDict;

        public CommandConsumer(string groupName) : this(ConsumerSetting.Default, groupName) { }
        public CommandConsumer(ConsumerSetting setting, string groupName) : this(string.Format("CommandConsumer@{0}", SocketUtils.GetLocalIPV4()), setting, groupName) { }
        public CommandConsumer(string id, ConsumerSetting setting, string groupName)
        {
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
            _commandExecutor = ObjectContainer.Resolve<ICommandExecutor>();
            _repository = ObjectContainer.Resolve<IRepository>();
            _messageContextDict = new ConcurrentDictionary<Guid, IMessageContext>();
            _consumer = new Consumer(id, setting, groupName, MessageModel.Clustering, this);
        }

        public CommandConsumer Start()
        {
            _consumer.Start();
            return this;
        }
        public CommandConsumer Shutdown()
        {
            _consumer.Shutdown();
            return this;
        }

        void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var payload = ByteTypeDataUtils.Decode(message.Body);
            var type = _commandTypeCodeProvider.GetType(payload.TypeCode);
            var command = _binarySerializer.Deserialize(payload.Data, type) as ICommand;

            if (_messageContextDict.TryAdd(command.Id, context))
            {
                _commandExecutor.Execute(command, new CommandExecuteContext(_repository, message, (executedCommand, queueMessage) =>
                {
                    IMessageContext messageContext;
                    if (_messageContextDict.TryRemove(executedCommand.Id, out messageContext))
                    {
                        messageContext.OnMessageHandled(queueMessage);
                    }
                }));
            }
        }
    }
}
