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
        private readonly IBinarySerializer _binarySerializer;
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
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
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

        void IQueueMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var commandMessage = _binarySerializer.Deserialize<CommandMessage>(message.Body);
            var payload = ByteTypeDataUtils.Decode(commandMessage.CommandData);
            var type = _commandTypeCodeProvider.GetType(payload.TypeCode);
            var command = _binarySerializer.Deserialize(payload.Data, type) as ICommand;

            command.Items["DomainEventHandledMessageTopic"] = commandMessage.DomainEventHandledMessageTopic;
            command.Items["SourceEventId"] = commandMessage.SourceEventId;
            command.Items["SourceExceptionId"] = commandMessage.SourceExceptionId;
            _commandExecutor.Execute(new ProcessingCommand(command, new CommandExecuteContext(_repository, message, context, commandMessage, CommandExecutedCallback)));
        }

        private void CommandExecutedCallback(ICommand command, CommandStatus commandStatus, string aggregateRootId, string exceptionTypeName, string errorMessage, CommandExecuteContext commandExecuteContext)
        {
            commandExecuteContext.MessageContext.OnMessageHandled(commandExecuteContext.QueueMessage);

            if (string.IsNullOrEmpty(commandExecuteContext.CommandMessage.CommandExecutedMessageTopic))
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
                Items = command.Items ?? new Dictionary<string, string>()
            }, commandExecuteContext.CommandMessage.CommandExecutedMessageTopic);
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IEvent> _events;
            private readonly ConcurrentDictionary<string, IAggregateRoot> _trackingAggregateRoots;
            private readonly IRepository _repository;

            public Action<ICommand, CommandStatus, string, string, string, CommandExecuteContext> CommandExecutedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }
            public IMessageContext MessageContext { get; private set; }
            public CommandMessage CommandMessage { get; private set; }

            public CommandExecuteContext(IRepository repository, QueueMessage queueMessage, IMessageContext messageContext, CommandMessage commandMessage, Action<ICommand, CommandStatus, string, string, string, CommandExecuteContext> commandExecutedAction)
            {
                _events = new ConcurrentDictionary<string, IEvent>();
                _trackingAggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _repository = repository;
                QueueMessage = queueMessage;
                CommandMessage = commandMessage;
                MessageContext = messageContext;
                CheckCommandWaiting = true;
                CommandExecutedAction = commandExecutedAction;
            }

            public bool CheckCommandWaiting { get; set; }
            public void OnCommandExecuted(ICommand command, CommandStatus commandStatus, string aggregateRootId, string exceptionTypeName, string errorMessage)
            {
                CommandExecutedAction(command, commandStatus, aggregateRootId, exceptionTypeName, errorMessage, this);
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

                if (aggregateRoot != null)
                {
                    _trackingAggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
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
