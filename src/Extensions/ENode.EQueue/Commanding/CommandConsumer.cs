using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using EQueue.Utils;

namespace ENode.EQueue
{
    public class CommandConsumer : IMessageHandler
    {
        private const string DefaultCommandConsumerId = "CommandConsumer";
        private const string DefaultCommandConsumerGroup = "CommandConsumerGroup";
        private readonly Consumer _consumer;
        private readonly CommandExecutedMessageSender _commandExecutedMessageSender;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IRepository _repository;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IMessageContext> _messageContextDict;

        public Consumer Consumer { get { return _consumer; } }

        public CommandConsumer(
            string id = null,
            string groupName = null,
            ConsumerSetting setting = null,
            CommandExecutedMessageSender commandExecutedMessageSender = null)
        {
            _consumer = new Consumer(id ?? DefaultCommandConsumerId, groupName ?? DefaultCommandConsumerGroup, setting ?? new ConsumerSetting());
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
            _commandExecutor = ObjectContainer.Resolve<ICommandExecutor>();
            _repository = ObjectContainer.Resolve<IRepository>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _messageContextDict = new ConcurrentDictionary<string, IMessageContext>();
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

        void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var commandMessage = _binarySerializer.Deserialize<CommandMessage>(message.Body);
            var payload = ByteTypeDataUtils.Decode(commandMessage.CommandData);
            var type = _commandTypeCodeProvider.GetType(payload.TypeCode);
            var command = _binarySerializer.Deserialize(payload.Data, type) as ICommand;

            if (_messageContextDict.TryAdd(command.Id, context))
            {
                var items = new Dictionary<string, string>();
                items.Add("DomainEventHandledMessageTopic", commandMessage.DomainEventHandledMessageTopic);
                items.Add("SourceEventId", commandMessage.SourceEventId);
                _commandExecutor.Execute(new ProcessingCommand(command, new CommandExecuteContext(_repository, message, commandMessage, items, CommandExecutedCallback)));
            }
            else
            {
                _logger.ErrorFormat("Duplicated command message. commandType:{0}, commandId:{1}", command.GetType().Name, command.Id);
            }
        }

        private void CommandExecutedCallback(ICommand command, CommandStatus commandStatus, string processId, string aggregateRootId, string exceptionTypeName, string errorMessage, CommandExecuteContext commandExecuteContext)
        {
            IMessageContext messageContext;
            if (_messageContextDict.TryRemove(command.Id, out messageContext))
            {
                messageContext.OnMessageHandled(commandExecuteContext.QueueMessage);
            }
            if (string.IsNullOrEmpty(commandExecuteContext.CommandMessage.CommandExecutedMessageTopic))
            {
                return;
            }

            _commandExecutedMessageSender.Send(new CommandExecutedMessage
            {
                CommandId = command.Id,
                AggregateRootId = aggregateRootId,
                ProcessId = processId,
                CommandStatus = commandStatus,
                ExceptionTypeName = exceptionTypeName,
                ErrorMessage = errorMessage,
                Items = command.Items ?? new Dictionary<string, string>()
            }, commandExecuteContext.CommandMessage.CommandExecutedMessageTopic);
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IEvent> _events;
            private readonly ConcurrentDictionary<string, IAggregateRoot> _trackingAggregateRoots;
            private readonly IRepository _repository;

            public Action<ICommand, CommandStatus, string, string, string, string, CommandExecuteContext> CommandExecutedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }
            public CommandMessage CommandMessage { get; private set; }
            public IDictionary<string, string> Items { get; private set; }

            public CommandExecuteContext(IRepository repository, QueueMessage queueMessage, CommandMessage commandMessage, IDictionary<string, string> items, Action<ICommand, CommandStatus, string, string, string, string, CommandExecuteContext> commandExecutedAction)
            {
                _events = new ConcurrentDictionary<string, IEvent>();
                _trackingAggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _repository = repository;
                QueueMessage = queueMessage;
                CommandMessage = commandMessage;
                Items = items;
                CheckCommandWaiting = true;
                CommandExecutedAction = commandExecutedAction;
            }

            public bool CheckCommandWaiting { get; set; }
            public void OnCommandExecuted(ICommand command, CommandStatus commandStatus, string processId, string aggregateRootId, string exceptionTypeName, string errorMessage)
            {
                CommandExecutedAction(command, commandStatus, processId, aggregateRootId, exceptionTypeName, errorMessage, this);
            }

            public void Add(IAggregateRoot aggregateRoot)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                if (!_trackingAggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot))
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
                if (_trackingAggregateRoots.TryGetValue(id.ToString(), out aggregateRoot))
                {
                    return aggregateRoot as T;
                }

                aggregateRoot = _repository.Get<T>(id);

                _trackingAggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot);

                return aggregateRoot as T;
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
                return _trackingAggregateRoots.Values;
            }
            public IEnumerable<IEvent> GetEvents()
            {
                return _events.Values;
            }
            public void Clear()
            {
                _trackingAggregateRoots.Clear();
                _events.Clear();
            }
        }
    }
}
