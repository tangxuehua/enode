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
using EQueue.Utils;
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
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IRepository _repository;
        private readonly ILogger _logger;

        public Consumer Consumer { get { return _consumer; } }

        public CommandConsumer(
            string id = null,
            string groupName = null,
            ConsumerSetting setting = null,
            CommandExecutedMessageSender commandExecutedMessageSender = null)
        {
            _consumer = new Consumer(id ?? DefaultCommandConsumerId, groupName ?? DefaultCommandConsumerGroup, setting ?? new ConsumerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<ICommand>>();
            _commandExecutor = ObjectContainer.Resolve<ICommandExecutor>();
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
            var commandMessage = _jsonSerializer.Deserialize<CommandMessage>(Encoding.UTF8.GetString(queueMessage.Body));
            var commandType = _commandTypeCodeProvider.GetType(commandMessage.CommandTypeCode);
            var command = _jsonSerializer.Deserialize(commandMessage.CommandData, commandType) as ICommand;
            var commandExecuteContext = new CommandExecuteContext(_repository, queueMessage, context, commandMessage, _commandExecutedMessageSender);
            var commandItems = new Dictionary<string, string>();

            commandItems["DomainEventHandledMessageTopic"] = commandMessage.DomainEventHandledMessageTopic;
            commandItems["SourceId"] = commandMessage.SourceId;
            commandItems["SourceType"] = commandMessage.SourceType;

            _commandExecutor.Execute(new ProcessingCommand(command, commandExecuteContext, commandItems));
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRoots;
            private readonly ConcurrentDictionary<string, IEvent> _events;
            private readonly IRepository _repository;
            private readonly CommandExecutedMessageSender _commandExecutedMessageSender;
            private readonly QueueMessage _queueMessage;
            private readonly IMessageContext _messageContext;
            private readonly CommandMessage _commandMessage;

            public bool CheckCommandWaiting { get; set; }

            public CommandExecuteContext(IRepository repository, QueueMessage queueMessage, IMessageContext messageContext, CommandMessage commandMessage, CommandExecutedMessageSender commandExecutedMessageSender)
            {
                _aggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _events = new ConcurrentDictionary<string, IEvent>();
                _repository = repository;
                _commandExecutedMessageSender = commandExecutedMessageSender;
                _queueMessage = queueMessage;
                _commandMessage = commandMessage;
                _messageContext = messageContext;
                CheckCommandWaiting = true;
            }

            public void OnCommandExecuted(ICommand command, CommandStatus commandStatus, string aggregateRootId, string exceptionTypeName, string errorMessage)
            {
                _messageContext.OnMessageHandled(_queueMessage);

                if (string.IsNullOrEmpty(_commandMessage.CommandExecutedMessageTopic))
                {
                    return;
                }

                _commandExecutedMessageSender.Send(new CommandExecutedMessage
                {
                    CommandId = command.Id,
                    AggregateRootId = aggregateRootId,
                    CommandStatus = commandStatus,
                    ExceptionTypeName = exceptionTypeName,
                    ErrorMessage = errorMessage,
                }, _commandMessage.CommandExecutedMessageTopic);
            }
            public void Add(IAggregateRoot aggregateRoot)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                if (!_aggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot))
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
                if (_aggregateRoots.TryGetValue(id.ToString(), out aggregateRoot))
                {
                    return aggregateRoot as T;
                }

                aggregateRoot = _repository.Get<T>(id);

                if (aggregateRoot != null)
                {
                    _aggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
                    return aggregateRoot as T;
                }

                return null;
            }
            public void Add(IEvent evnt)
            {
                if (evnt == null)
                {
                    throw new ArgumentNullException("evnt");
                }
                if (!_events.TryAdd(evnt.Id, evnt))
                {
                    throw new EventAlreadyExistException(evnt.Id, evnt.GetType());
                }
            }
            public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
            {
                return _aggregateRoots.Values;
            }
            public IEnumerable<IEvent> GetEvents()
            {
                return _events.Values;
            }
            public void Clear()
            {
                _aggregateRoots.Clear();
                _events.Clear();
            }
        }
    }
}
