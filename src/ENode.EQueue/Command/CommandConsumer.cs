using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class CommandConsumer : IQueueMessageHandler
    {
        private const string DefaultCommandConsumerId = "CommandConsumer";
        private const string DefaultCommandConsumerGroup = "CommandConsumerGroup";
        private readonly Consumer _consumer;
        private readonly CommandExecutedMessageSender _commandExecutedMessageSender;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly IMessageProcessor<ProcessingCommand, ICommand, CommandResult> _processor;
        private readonly IRepository _repository;
        private readonly ILogger _logger;

        public Consumer Consumer { get { return _consumer; } }

        public CommandConsumer(
            string id = null,
            string groupName = null,
            ConsumerSetting setting = null,
            CommandExecutedMessageSender commandExecutedMessageSender = null)
        {
            _consumer = new Consumer(id ?? DefaultCommandConsumerId, groupName ?? DefaultCommandConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential
            });
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _typeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider>();
            _processor = ObjectContainer.Resolve<IMessageProcessor<ProcessingCommand, ICommand, CommandResult>>();
            _repository = ObjectContainer.Resolve<IRepository>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _commandExecutedMessageSender = commandExecutedMessageSender ?? new CommandExecutedMessageSender();
        }

        public CommandConsumer Start()
        {
            _consumer.SetMessageHandler(this).Start();
            _commandExecutedMessageSender.Start();
            return this;
        }
        public CommandConsumer Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            return this;
        }
        public CommandConsumer Shutdown()
        {
            _consumer.Shutdown();
            _commandExecutedMessageSender.Shutdown();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var commandItems = new Dictionary<string, string>();
            var commandMessage = _jsonSerializer.Deserialize<CommandMessage>(Encoding.UTF8.GetString(queueMessage.Body));
            var commandType = _typeCodeProvider.GetType(commandMessage.CommandTypeCode);
            var command = _jsonSerializer.Deserialize(commandMessage.CommandData, commandType) as ICommand;
            var commandExecuteContext = new CommandExecuteContext(_repository, queueMessage, context, commandMessage, _commandExecutedMessageSender);
            commandItems["DomainEventHandledMessageTopic"] = commandMessage.DomainEventHandledMessageTopic;
            _processor.Process(new ProcessingCommand(command, commandExecuteContext, commandItems));
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _changedAggregateRootDict;
            private readonly IRepository _repository;
            private readonly CommandExecutedMessageSender _commandExecutedMessageSender;
            private readonly QueueMessage _queueMessage;
            private readonly IMessageContext _messageContext;
            private readonly CommandMessage _commandMessage;

            public CommandExecuteContext(IRepository repository, QueueMessage queueMessage, IMessageContext messageContext, CommandMessage commandMessage, CommandExecutedMessageSender commandExecutedMessageSender)
            {
                _changedAggregateRootDict = new ConcurrentDictionary<string, IAggregateRoot>();
                _repository = repository;
                _commandExecutedMessageSender = commandExecutedMessageSender;
                _queueMessage = queueMessage;
                _commandMessage = commandMessage;
                _messageContext = messageContext;
            }

            public void OnCommandExecuted(CommandResult commandResult)
            {
                _messageContext.OnMessageHandled(_queueMessage);

                if (string.IsNullOrEmpty(_commandMessage.CommandExecutedMessageTopic))
                {
                    return;
                }

                _commandExecutedMessageSender.SendAsync(new CommandExecutedMessage
                {
                    CommandId = commandResult.CommandId,
                    AggregateRootId = commandResult.AggregateRootId,
                    CommandStatus = commandResult.Status,
                    ExceptionTypeName = commandResult.ExceptionTypeName,
                    ErrorMessage = commandResult.ErrorMessage,
                }, _commandMessage.CommandExecutedMessageTopic);
            }
            public void Add(IAggregateRoot aggregateRoot)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                if (!_changedAggregateRootDict.TryAdd(aggregateRoot.UniqueId, aggregateRoot))
                {
                    throw new AggregateRootAlreadyExistException(aggregateRoot.UniqueId, aggregateRoot.GetType());
                }
            }
            public T Get<T>(object id) where T : class, IAggregateRoot
            {
                if (id == null)
                {
                    throw new ArgumentNullException("id");
                }

                IAggregateRoot aggregateRoot = null;
                if (_changedAggregateRootDict.TryGetValue(id.ToString(), out aggregateRoot))
                {
                    return aggregateRoot as T;
                }

                aggregateRoot = _repository.Get<T>(id);

                if (aggregateRoot != null)
                {
                    _changedAggregateRootDict.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
                    return aggregateRoot as T;
                }

                return null;
            }
            public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
            {
                return _changedAggregateRootDict.Values;
            }
            public void Clear()
            {
                _changedAggregateRootDict.Clear();
            }
        }
    }
}
