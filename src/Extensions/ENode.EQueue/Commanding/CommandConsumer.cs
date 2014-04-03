using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.IoC;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using EQueue.Utils;

namespace ENode.EQueue
{
    public class CommandConsumer : IMessageHandler
    {
        private readonly Consumer _consumer;
        private readonly CommandExecutedMessageSender _commandExecutedMessageSender;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IRepository _repository;
        private readonly ConcurrentDictionary<string, IMessageContext> _messageContextDict;

        public Consumer Consumer { get { return _consumer; } }

        public CommandConsumer(CommandExecutedMessageSender commandExecutedMessageSender)
            : this(null, commandExecutedMessageSender)
        {
        }
        public CommandConsumer(ConsumerSetting setting, CommandExecutedMessageSender commandExecutedMessageSender)
            : this("CommandConsumer", "CommandConsumerGroup", setting, commandExecutedMessageSender)
        {
        }
        public CommandConsumer(string id, string groupName, CommandExecutedMessageSender commandExecutedMessageSender)
            : this(id, groupName, new ConsumerSetting(), commandExecutedMessageSender)
        {
        }
        public CommandConsumer(string id, string groupName, ConsumerSetting setting, CommandExecutedMessageSender commandExecutedMessageSender)
        {
            _consumer = new Consumer(id, string.IsNullOrEmpty(groupName) ? typeof(CommandConsumer).Name + "Group" : groupName, setting);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
            _commandExecutor = ObjectContainer.Resolve<ICommandExecutor>();
            _repository = ObjectContainer.Resolve<IRepository>();
            _messageContextDict = new ConcurrentDictionary<string, IMessageContext>();
            _commandExecutedMessageSender = commandExecutedMessageSender;
        }

        public CommandConsumer Start()
        {
            _consumer.Start(this);
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
                _commandExecutor.Execute(new ProcessingCommand(command, new CommandExecuteContext(_repository, message, commandMessage, items, CommandExecutedCallback)));
            }
        }

        private void CommandExecutedCallback(ICommand command, CommandStatus commandStatus, int exceptionCode, string errorMessage, CommandExecuteContext commandExecuteContext)
        {
            IMessageContext messageContext;
            if (_messageContextDict.TryRemove(command.Id, out messageContext))
            {
                messageContext.OnMessageHandled(commandExecuteContext.QueueMessage);
            }

            _commandExecutedMessageSender.Send(new CommandExecutedMessage
            {
                CommandId = command.Id,
                AggregateRootId = command.AggregateRootId,
                ProcessId = command is IProcessCommand ? ((IProcessCommand)command).ProcessId : null,
                CommandStatus = commandStatus,
                ExceptionCode = exceptionCode,
                ErrorMessage = errorMessage
            }, commandExecuteContext.CommandMessage.CommandExecutedMessageTopic);
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _trackingAggregateRoots;
            private readonly IRepository _repository;

            public Action<ICommand, CommandStatus, int, string, CommandExecuteContext> CommandExecutedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }
            public CommandMessage CommandMessage { get; private set; }
            public IDictionary<string, string> Items { get; private set; }

            public CommandExecuteContext(IRepository repository, QueueMessage queueMessage, CommandMessage commandMessage, IDictionary<string, string> items, Action<ICommand, CommandStatus, int, string, CommandExecuteContext> commandExecutedAction)
            {
                _trackingAggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _repository = repository;
                QueueMessage = queueMessage;
                CommandMessage = commandMessage;
                Items = items;
                CheckCommandWaiting = true;
                CommandExecutedAction = commandExecutedAction;
            }

            public bool CheckCommandWaiting { get; set; }
            public void OnCommandExecuted(ICommand command, CommandStatus commandStatus, int exceptionCode, string errorMessage)
            {
                CommandExecutedAction(command, commandStatus, exceptionCode, errorMessage, this);
            }

            /// <summary>Add an aggregate root to the context.
            /// </summary>
            /// <param name="aggregateRoot">The aggregate root to add.</param>
            /// <exception cref="ArgumentNullException">Throwed when the aggregate root is null.</exception>
            public void Add(IAggregateRoot aggregateRoot)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                if (!_trackingAggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot))
                {
                    throw new ENodeException("Aggregate root [id={0}] has already been added to the current command context.", aggregateRoot.UniqueId);
                }
            }
            /// <summary>Get the aggregate from the context.
            /// </summary>
            /// <param name="id">The id of the aggregate root.</param>
            /// <typeparam name="T">The type of the aggregate root.</typeparam>
            /// <returns>The found aggregate root.</returns>
            /// <exception cref="ArgumentNullException">Throwed when the id is null.</exception>
            /// <exception cref="AggregateRootNotExistException">Throwed when the aggregate root not found.</exception>
            public T Get<T>(object id) where T : class, IAggregateRoot
            {
                if (id == null)
                {
                    throw new ArgumentNullException("id");
                }

                var aggregateRoot = _repository.Get<T>(id);

                if (aggregateRoot == null)
                {
                    throw new AggregateRootNotExistException(id, typeof(T));
                }
                _trackingAggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
                return aggregateRoot;
            }
            /// <summary>Returns all the tracked aggregate roots of the current context.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
            {
                return _trackingAggregateRoots.Values;
            }
            /// <summary>Clear all the tracking aggregates.
            /// </summary>
            public void Clear()
            {
                _trackingAggregateRoots.Clear();
            }
        }
    }
}
