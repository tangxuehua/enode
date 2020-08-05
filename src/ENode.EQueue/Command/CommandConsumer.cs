using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class CommandConsumer : IQueueMessageHandler
    {
        private const string DefaultCommandConsumerGroup = "CommandConsumerGroup";
        private SendReplyService _sendReplyService;
        private IJsonSerializer _jsonSerializer;
        private ITypeNameProvider _typeNameProvider;
        private ICommandProcessor _commandProcessor;
        private IRepository _repository;
        private IAggregateStorage _aggregateStorage;
        private ILogger _logger;

        public Consumer Consumer { get; private set; }

        public CommandConsumer InitializeENode()
        {
            _sendReplyService = new SendReplyService("CommandConsumerSendReplyService");
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _commandProcessor = ObjectContainer.Resolve<ICommandProcessor>();
            _repository = ObjectContainer.Resolve<IRepository>();
            _aggregateStorage = ObjectContainer.Resolve<IAggregateStorage>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            return this;
        }
        public CommandConsumer InitializeEQueue(string groupName = null, ConsumerSetting setting = null)
        {
            InitializeENode();
            Consumer = new Consumer(groupName ?? DefaultCommandConsumerGroup, setting ?? new ConsumerSetting
            {
                ConsumeFromWhere = ConsumeFromWhere.FirstOffset
            }, "CommandConsumer");
            return this;
        }

        public CommandConsumer Start()
        {
            _sendReplyService.Start();
            Consumer.SetMessageHandler(this).Start();
            return this;
        }
        public CommandConsumer Subscribe(string topic)
        {
            Consumer.Subscribe(topic);
            return this;
        }
        public CommandConsumer Shutdown()
        {
            Consumer.Stop();
            _sendReplyService.Stop();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var commandMessageString = Encoding.UTF8.GetString(queueMessage.Body);

            _logger.InfoFormat("Received command equeue message: {0}, commandMessage: {1}", queueMessage, commandMessageString);

            var commandItems = new Dictionary<string, string>();
            var commandMessage = _jsonSerializer.Deserialize<CommandMessage>(commandMessageString);
            var commandType = _typeNameProvider.GetType(queueMessage.Tag);
            var command = _jsonSerializer.Deserialize(commandMessage.CommandData, commandType) as ICommand;
            var commandExecuteContext = new CommandExecuteContext(_repository, _aggregateStorage, queueMessage, context, commandMessage, _sendReplyService);
            commandItems["CommandReplyAddress"] = commandMessage.ReplyAddress;
            _commandProcessor.Process(new ProcessingCommand(command, commandExecuteContext, commandItems));
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private IApplicationMessage _applicationMessage;
            private string _result;
            private readonly ConcurrentDictionary<string, IAggregateRoot> _trackingAggregateRootDict;
            private readonly IRepository _repository;
            private readonly IAggregateStorage _aggregateRootStorage;
            private readonly SendReplyService _sendReplyService;
            private readonly QueueMessage _queueMessage;
            private readonly IMessageContext _messageContext;
            private readonly CommandMessage _commandMessage;

            public CommandExecuteContext(IRepository repository, IAggregateStorage aggregateRootStorage, QueueMessage queueMessage, IMessageContext messageContext, CommandMessage commandMessage, SendReplyService sendReplyService)
            {
                _trackingAggregateRootDict = new ConcurrentDictionary<string, IAggregateRoot>();
                _repository = repository;
                _aggregateRootStorage = aggregateRootStorage;
                _sendReplyService = sendReplyService;
                _queueMessage = queueMessage;
                _commandMessage = commandMessage;
                _messageContext = messageContext;
            }

            public Task OnCommandExecutedAsync(CommandResult commandResult)
            {
                _messageContext.OnMessageHandled(_queueMessage);

                if (string.IsNullOrEmpty(_commandMessage.ReplyAddress))
                {
                    return Task.CompletedTask;
                }

                return _sendReplyService.SendReply((int)CommandReturnType.CommandExecuted, commandResult, _commandMessage.ReplyAddress);
            }
            public void Add(IAggregateRoot aggregateRoot)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                if (!_trackingAggregateRootDict.TryAdd(aggregateRoot.UniqueId, aggregateRoot))
                {
                    throw new AggregateRootAlreadyExistException(aggregateRoot.UniqueId, aggregateRoot.GetType());
                }
            }
            public Task AddAsync(IAggregateRoot aggregateRoot)
            {
                Add(aggregateRoot);
                return Task.CompletedTask;
            }
            public async Task<T> GetAsync<T>(object id, bool firstFromCache = true) where T : class, IAggregateRoot
            {
                if (id == null)
                {
                    throw new ArgumentNullException("id");
                }

                var aggregateRootId = id.ToString();
                if (_trackingAggregateRootDict.TryGetValue(aggregateRootId, out IAggregateRoot aggregateRoot))
                {
                    return aggregateRoot as T;
                }

                if (firstFromCache)
                {
                    aggregateRoot = await _repository.GetAsync<T>(id).ConfigureAwait(false);
                }
                else
                {
                    aggregateRoot = await _aggregateRootStorage.GetAsync(typeof(T), aggregateRootId).ConfigureAwait(false);
                }

                if (aggregateRoot != null)
                {
                    _trackingAggregateRootDict.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
                    return aggregateRoot as T;
                }

                return null;
            }
            public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
            {
                return _trackingAggregateRootDict.Values;
            }
            public void Clear()
            {
                _trackingAggregateRootDict.Clear();
                _result = null;
            }
            public void SetResult(string result)
            {
                _result = result;
            }
            public string GetResult()
            {
                return _result;
            }

            public void SetApplicationMessage(IApplicationMessage applicationMessage)
            {
                _applicationMessage = applicationMessage;
            }

            public IApplicationMessage GetApplicationMessage()
            {
                return _applicationMessage;
            }
        }
    }
}
