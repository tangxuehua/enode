using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
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
        private readonly Consumer _consumer;
        private readonly FailedCommandMessageSender _failedCommandMessageSender;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ICommandTypeCodeProvider _commandTypeCodeProvider;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IRepository _repository;
        private readonly ConcurrentDictionary<Guid, IMessageContext> _messageContextDict;

        public Consumer Consumer { get { return _consumer; } }

        public CommandConsumer(FailedCommandMessageSender failedCommandMessageSender)
            : this(new ConsumerSetting(), failedCommandMessageSender)
        {
        }
        public CommandConsumer(string groupName, FailedCommandMessageSender failedCommandMessageSender)
            : this(new ConsumerSetting(), groupName, failedCommandMessageSender)
        {
        }
        public CommandConsumer(ConsumerSetting setting, FailedCommandMessageSender failedCommandMessageSender)
            : this(setting, null, failedCommandMessageSender)
        {
        }
        public CommandConsumer(ConsumerSetting setting, string groupName, FailedCommandMessageSender failedCommandMessageSender)
            : this(setting, null, groupName, failedCommandMessageSender)
        {
        }
        public CommandConsumer(ConsumerSetting setting, string name, string groupName, FailedCommandMessageSender failedCommandMessageSender)
            : this(string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(CommandConsumer).Name : name, ObjectId.GenerateNewId()), setting, groupName, failedCommandMessageSender)
        {
        }
        public CommandConsumer(string id, ConsumerSetting setting, string groupName, FailedCommandMessageSender failedCommandMessageSender)
        {
            _consumer = new Consumer(id, setting, string.IsNullOrEmpty(groupName) ? typeof(CommandConsumer).Name + "Group" : groupName);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _commandTypeCodeProvider = ObjectContainer.Resolve<ICommandTypeCodeProvider>();
            _commandExecutor = ObjectContainer.Resolve<ICommandExecutor>();
            _repository = ObjectContainer.Resolve<IRepository>();
            _messageContextDict = new ConcurrentDictionary<Guid, IMessageContext>();
            _failedCommandMessageSender = failedCommandMessageSender;
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
                _commandExecutor.Execute(new ProcessingCommand(command, new CommandExecuteContext(_repository, message, commandMessage, items, CommandHandledCallback)));
            }
        }

        private void CommandHandledCallback(ICommand command, string errorMessage, CommandExecuteContext commandExecuteContext)
        {
            IMessageContext messageContext;
            if (_messageContextDict.TryRemove(command.Id, out messageContext))
            {
                messageContext.OnMessageHandled(commandExecuteContext.QueueMessage);
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _failedCommandMessageSender.Send(new FailedCommandMessage
                {
                    CommandId = command.Id,
                    AggregateRootId = command.AggregateRootId,
                    ProcessId = command is IProcessCommand ? ((IProcessCommand)command).ProcessId : null,
                    ErrorMessage = errorMessage
                }, commandExecuteContext.CommandMessage.FailedCommandMessageTopic);
            }
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<object, IAggregateRoot> _trackingAggregateRoots;
            private readonly IRepository _repository;

            public Action<ICommand, string, CommandExecuteContext> CommandExecutedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }
            public CommandMessage CommandMessage { get; private set; }
            public IDictionary<string, string> Items { get; private set; }

            public CommandExecuteContext(IRepository repository, QueueMessage queueMessage, CommandMessage commandMessage, IDictionary<string, string> items, Action<ICommand, string, CommandExecuteContext> commandExecutedAction)
            {
                _trackingAggregateRoots = new ConcurrentDictionary<object, IAggregateRoot>();
                _repository = repository;
                QueueMessage = queueMessage;
                CommandMessage = commandMessage;
                Items = items;
                CheckCommandWaiting = true;
                CommandExecutedAction = commandExecutedAction;
            }

            public bool CheckCommandWaiting { get; set; }
            public void OnCommandExecuted(ICommand command, string errorMessage)
            {
                CommandExecutedAction(command, errorMessage, this);
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
                var firstSourcingEvent = aggregateRoot.GetUncommittedEvents().FirstOrDefault(x => x is ISourcingEvent);
                if (firstSourcingEvent == null)
                {
                    throw new NotSupportedException(string.Format("Aggregate [{0}] has no sourcing event, cannot be added.", aggregateRoot.GetType()));
                }

                _trackingAggregateRoots.TryAdd(firstSourcingEvent.AggregateRootId, aggregateRoot);
            }
            /// <summary>Get the aggregate from the context.
            /// </summary>
            /// <param name="id">The id of the aggregate root.</param>
            /// <typeparam name="T">The type of the aggregate root.</typeparam>
            /// <returns>The found aggregate root.</returns>
            /// <exception cref="ArgumentNullException">Throwed when the id is null.</exception>
            /// <exception cref="AggregateRootNotFoundException">Throwed when the aggregate root not found.</exception>
            public T Get<T>(object id) where T : class, IAggregateRoot
            {
                var aggregateRoot = GetOrDefault<T>(id);

                if (aggregateRoot == null)
                {
                    throw new AggregateRootNotFoundException(id, typeof(T));
                }

                return aggregateRoot;
            }
            /// <summary>Get the aggregate from the context, if the aggregate root not exist, returns null.
            /// </summary>
            /// <param name="id">The id of the aggregate root.</param>
            /// <typeparam name="T">The type of the aggregate root.</typeparam>
            /// <returns>If the aggregate root was found, then returns it; otherwise, returns null.</returns>
            /// <exception cref="ArgumentNullException">Throwed when the id is null.</exception>
            public T GetOrDefault<T>(object id) where T : class, IAggregateRoot
            {
                if (id == null)
                {
                    throw new ArgumentNullException("id");
                }

                var aggregateRoot = _repository.Get<T>(id);

                if (aggregateRoot != null)
                {
                    _trackingAggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
                }

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
