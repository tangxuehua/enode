using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Configurations;
using ECommon.IoC;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Serializing;
using ENode.Commanding;
using ENode.Commanding.Impl;
using ENode.Domain;
using ENode.Domain.Impl;
using ENode.Eventing;
using ENode.Eventing.Impl;
using ENode.Eventing.Impl.InMemory;
using ENode.Eventing.Impl.SQL;
using ENode.Infrastructure;
using ENode.Infrastructure.Logging;
using ENode.Infrastructure.Sql;
using ENode.Messaging;
using ENode.Messaging.Impl;
using ENode.Messaging.Impl.SQL;
using ENode.Snapshoting;
using ENode.Snapshoting.Impl;

namespace ENode.Configurations
{
    /// <summary>Configuration class for enode framework.
    /// </summary>
    public class ENodeConfiguration
    {
        #region Private Vairables

        private readonly Configuration _configuration;
        private readonly IList<Type> _assemblyInitializerServiceTypes;
        private readonly IList<ICommandProcessor> _commandProcessors;
        private ICommandProcessor _retryCommandProcessor;
        private ICommandProcessor _waitingCommandProcessor;
        private readonly IList<IUncommittedEventProcessor> _uncommittedEventProcessors;
        private readonly IList<ICommittedEventProcessor> _committedEventProcessors;

        #endregion

        public static ENodeConfiguration Instance { get; private set; }

        public static ENodeConfiguration CreateENode(Configuration configuration)
        {
            if (Instance != null)
            {
                throw new Exception("Could not create enode configuration instance twice.");
            }
            Instance = new ENodeConfiguration(configuration);
            return Instance;
        }

        /// <summary>Get all the command queues.
        /// </summary>
        public IEnumerable<ICommandQueue> GetCommandQueues()
        {
            return _commandProcessors.Select(x => x.BindingQueue);
        }
        /// <summary>Get the retry command queue.
        /// </summary>
        public ICommandQueue GetRetryCommandQueue()
        {
            if (_retryCommandProcessor == null)
            {
                throw new Exception("The command queue for command retring is not configured.");
            }
            return _retryCommandProcessor.BindingQueue;
        }
        /// <summary>Get the waiting command queue.
        /// </summary>
        public ICommandQueue GetWaitingCommandQueue()
        {
            if (_waitingCommandProcessor == null)
            {
                throw new Exception("The command queue for waiting command is not configured.");
            }
            return _waitingCommandProcessor.BindingQueue;
        }
        /// <summary>Get all the uncommitted event queues.
        /// </summary>
        public IEnumerable<IUncommittedEventQueue> GetUncommitedEventQueues()
        {
            return _uncommittedEventProcessors.Select(x => x.BindingQueue);
        }
        /// <summary>Get all the committed event queues.
        /// </summary>
        public IEnumerable<ICommittedEventQueue> GetCommitedEventQueues()
        {
            return _committedEventProcessors.Select(x => x.BindingQueue);
        }

        /// <summary>Parameterized constructor.
        /// </summary>
        private ENodeConfiguration(Configuration configuration)
        {
            _configuration = configuration;
            _assemblyInitializerServiceTypes = new List<Type>();
            _commandProcessors = new List<ICommandProcessor>();
            _uncommittedEventProcessors = new List<IUncommittedEventProcessor>();
            _committedEventProcessors = new List<ICommittedEventProcessor>();
        }

        /// <summary>Register all the default components of enode framework.
        /// </summary>
        public ENodeConfiguration RegisterENodeComponents()
        {
            _configuration.SetDefault<ILoggerFactory, EmptyLoggerFactory>();
            _configuration.SetDefault<IBinarySerializer, DefaultBinarySerializer>();
            _configuration.SetDefault<IDbConnectionFactory, DefaultDbConnectionFactory>();
            _configuration.SetDefault<IMessageStore, EmptyMessageStore>();

            _configuration.SetDefault<IAggregateRootTypeProvider, DefaultAggregateRootTypeProvider>();
            _configuration.SetDefault<IAggregateRootInternalHandlerProvider, DefaultAggregateRootInternalHandlerProvider>();
            _configuration.SetDefault<IEventSourcingService, DefaultEventSourcingService>();
            _configuration.SetDefault<IAggregateRootFactory, DefaultAggregateRootFactory>();
            _configuration.SetDefault<IMemoryCache, DefaultMemoryCache>();
            _configuration.SetDefault<IAggregateStorage, EventSourcingAggregateStorage>();
            _configuration.SetDefault<IRepository, EventSourcingRepository>();
            _configuration.SetDefault<IMemoryCacheRebuilder, DefaultMemoryCacheRebuilder>();

            _configuration.SetDefault<ISnapshotter, DefaultSnapshotter>();
            _configuration.SetDefault<ISnapshotPolicy, NoSnapshotPolicy>();
            _configuration.SetDefault<ISnapshotStore, EmptySnapshotStore>();

            _configuration.SetDefault<ICommandHandlerProvider, DefaultCommandHandlerProvider>();
            _configuration.SetDefault<ICommandQueueRouter, DefaultCommandQueueRouter>();
            _configuration.SetDefault<IProcessingCommandCache, DefaultProcessingCommandCache>();
            _configuration.SetDefault<IWaitingCommandCache, DefaultWaitingCommandCache>();
            _configuration.SetDefault<IWaitingCommandService, DefaultWaitingCommandService>();
            _configuration.SetDefault<ICommandTaskManager, DefaultCommandTaskManager>();
            _configuration.SetDefault<ICommandCompletionEventManager, DefaultCommandCompletionEventManager>();
            _configuration.SetDefault<ICommandService, DefaultCommandService>();
            _configuration.SetDefault<IRetryCommandService, DefaultRetryCommandService>();

            _configuration.SetDefault<IEventHandlerProvider, DefaultEventHandlerProvider>();
            _configuration.SetDefault<IEventSynchronizerProvider, DefaultEventSynchronizerProvider>();
            _configuration.SetDefault<IEventStore, InMemoryEventStore>();
            _configuration.SetDefault<IEventPublishInfoStore, InMemoryEventPublishInfoStore>();
            _configuration.SetDefault<IEventHandleInfoStore, InMemoryEventHandleInfoStore>();
            _configuration.SetDefault<IEventHandleInfoCache, InMemoryEventHandleInfoCache>();
            _configuration.SetDefault<IUncommittedEventQueueRouter, DefaultUncommittedEventQueueRouter>();
            _configuration.SetDefault<ICommittedEventQueueRouter, DefaultCommittedEventQueueRouter>();
            _configuration.SetDefault<IEventTableNameProvider, AggregatePerEventTableNameProvider>();
            _configuration.SetDefault<IUncommittedEventSender, DefaultUncommittedEventSender>();
            _configuration.SetDefault<IEventPublisher, DefaultEventPublisher>();

            _configuration.SetDefault<IActionExecutionService, DefaultActionExecutionService>(LifeStyle.Transient);
            _configuration.SetDefault<ICommandContext, DefaultCommandContext>(LifeStyle.Transient);
            _configuration.SetDefault<ICommandMessageHandler, DefaultCommandMessageHandler>(LifeStyle.Transient);
            _configuration.SetDefault<IWaitingCommandMessageHandler, DefaultWaitingCommandMessageHandler>(LifeStyle.Transient);
            _configuration.SetDefault<IUncommittedEventMessageHandler, DefaultUncommittedEventMessageHandler>(LifeStyle.Transient);
            _configuration.SetDefault<ICommittedEventMessageHandler, DefaultCommittedEventMessageHandler>(LifeStyle.Transient);

            _assemblyInitializerServiceTypes.Add(typeof(IEventSourcingService));
            _assemblyInitializerServiceTypes.Add(typeof(IEventSynchronizerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IEventHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(ICommandHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IAggregateRootTypeProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IAggregateRootInternalHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(ICommandCompletionEventManager));

            return this;
        }
        /// <summary>Register all the business components from the given assemblies.
        /// </summary>
        public ENodeConfiguration RegisterBusinessComponents(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(TypeUtils.IsComponent))
                {
                    var life = ParseLife(type);
                    ObjectContainer.RegisterType(type, life);
                    foreach (var interfaceType in type.GetInterfaces())
                    {
                        ObjectContainer.RegisterType(interfaceType, type, life);
                    }
                    if (IsAssemblyInitializer(type))
                    {
                        _assemblyInitializerServiceTypes.Add(type);
                    }
                }
            }
            return this;
        }

        /// <summary>Use SQL DB as the storage of the whole framework.
        /// </summary>
        /// <param name="connectionString">The connection string of the DB.</param>
        /// <returns></returns>
        public ENodeConfiguration UseSql(string connectionString)
        {
            return UseSql(connectionString, "Event", null, "EventPublishInfo", "EventHandleInfo");
        }
        /// <summary>Use SQL DB as the storage of the whole framework.
        /// </summary>
        /// <param name="connectionString">The connection string of the DB.</param>
        /// <param name="eventTable">The table used to store all the domain events.</param>
        /// <param name="queueNameFormat">The format of the queue name.</param>
        /// <param name="eventPublishInfoTable">The table used to store all the event publish information.</param>
        /// <param name="eventHandleInfoTable">The table used to store all the event handle information.</param>
        /// <returns></returns>
        public ENodeConfiguration UseSql(string connectionString, string eventTable, string queueNameFormat, string eventPublishInfoTable, string eventHandleInfoTable)
        {
            _configuration.SetDefault<IEventTableNameProvider, DefaultEventTableNameProvider>(new DefaultEventTableNameProvider(eventTable));
            _configuration.SetDefault<IQueueTableNameProvider, DefaultQueueTableNameProvider>(new DefaultQueueTableNameProvider(queueNameFormat));
            _configuration.SetDefault<IMessageStore, SqlMessageStore>(new SqlMessageStore(connectionString));
            _configuration.SetDefault<IEventStore, SqlEventStore>(new SqlEventStore(connectionString));
            _configuration.SetDefault<IEventPublishInfoStore, SqlEventPublishInfoStore>(new SqlEventPublishInfoStore(connectionString, eventPublishInfoTable));
            _configuration.SetDefault<IEventHandleInfoStore, SqlEventHandleInfoStore>(new SqlEventHandleInfoStore(connectionString, eventHandleInfoTable));
            return this;
        }
        /// <summary>Use the default sql querydb connection factory.
        /// </summary>
        /// <param name="connectionString">The connection string of the SQL DB.</param>
        /// <returns></returns>
        public ENodeConfiguration UseDefaultSqlQueryDbConnectionFactory(string connectionString)
        {
            _configuration.SetDefault<ISqlQueryDbConnectionFactory, DefaultSqlQueryDbConnectionFactory>(new DefaultSqlQueryDbConnectionFactory(connectionString));
            return this;
        }

        /// <summary>Add a command processor.
        /// </summary>
        /// <param name="commandProcessor"></param>
        /// <returns></returns>
        public ENodeConfiguration AddCommandProcessor(ICommandProcessor commandProcessor)
        {
            _commandProcessors.Add(commandProcessor);
            return this;
        }
        /// <summary>Set the command processor to process the retried command.
        /// </summary>
        /// <param name="commandProcessor"></param>
        /// <returns></returns>
        public ENodeConfiguration SetRetryCommandProcessor(ICommandProcessor commandProcessor)
        {
            _retryCommandProcessor = commandProcessor;
            return this;
        }
        /// <summary>Set the command processor to process the waiting command.
        /// </summary>
        /// <param name="commandProcessor"></param>
        /// <returns></returns>
        public ENodeConfiguration SetWaitingCommandProcessor(ICommandProcessor commandProcessor)
        {
            _waitingCommandProcessor = commandProcessor;
            return this;
        }
        /// <summary>Add an uncommitted event processor.
        /// </summary>
        /// <param name="eventProcessor"></param>
        /// <returns></returns>
        public ENodeConfiguration AddUncommittedEventProcessor(IUncommittedEventProcessor eventProcessor)
        {
            _uncommittedEventProcessors.Add(eventProcessor);
            return this;
        }
        /// <summary>Add a committed event processor.
        /// </summary>
        /// <param name="eventProcessor"></param>
        /// <returns></returns>
        public ENodeConfiguration AddCommittedEventProcessor(ICommittedEventProcessor eventProcessor)
        {
            _committedEventProcessors.Add(eventProcessor);
            return this;
        }
        /// <summary>Create all the message processors with the default queue names at once.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration CreateAllDefaultProcessors()
        {
            return CreateAllDefaultProcessors(
                new string[] { "CommandQueue" },
                "RetryCommandQueue",
                "WaitingCommandQueue",
                new string[] { "UncommittedEventQueue" },
                new string[] { "CommittedEventQueue" });
        }

        /// <summary>Create all the message processors with the given queue names at once.
        /// </summary>
        /// <param name="commandQueueNames">Represents the command queue names.</param>
        /// <param name="retryCommandQueueName">Represents the retry command queue name.</param>
        /// <param name="waitingCommandQueueName">Represents the waiting command queue name.</param>
        /// <param name="uncommittedEventQueueNames">Represents the uncommitted event queue names.</param>
        /// <param name="committedEventQueueNames">Represents the committed event queue names.</param>
        /// <param name="option">The message processor creation option.</param>
        /// <returns></returns>
        public ENodeConfiguration CreateAllDefaultProcessors(
            IEnumerable<string> commandQueueNames,
            string retryCommandQueueName,
            string waitingCommandQueueName,
            IEnumerable<string> uncommittedEventQueueNames,
            IEnumerable<string> committedEventQueueNames,
            MessageProcessorOption option = null)
        {
            var messageProcessorOption = option ?? MessageProcessorOption.Default;

            foreach (var queueName in commandQueueNames)
            {
                _commandProcessors.Add(new DefaultCommandProcessor(new DefaultCommandQueue(queueName), messageProcessorOption.CommandExecutorCount));
            }

            _retryCommandProcessor = new DefaultCommandProcessor(
                new DefaultCommandQueue(retryCommandQueueName),
                messageProcessorOption.RetryCommandExecutorCount,
                messageProcessorOption.RetryCommandDequeueIntervalMilliseconds);

            _waitingCommandProcessor = new DefaultWaitingCommandProcessor(
                new DefaultCommandQueue(waitingCommandQueueName),
                messageProcessorOption.WaitingCommandExecutorCount);

            foreach (var queueName in uncommittedEventQueueNames)
            {
                _uncommittedEventProcessors.Add(new DefaultUncommittedEventProcessor(new DefaultUncommittedEventQueue(queueName), messageProcessorOption.UncommittedEventExecutorCount));
            }

            foreach (var queueName in committedEventQueueNames)
            {
                _committedEventProcessors.Add(new DefaultCommittedEventProcessor(new DefaultCommittedEventQueue(queueName), messageProcessorOption.CommittedEventExecutorCount));
            }

            return this;
        }
        /// <summary>Initialize all the assembly initializers with the given assemblies.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration Initialize(params Assembly[] assemblies)
        {
            ValidateSerializableTypes(assemblies);
            foreach (var assemblyInitializer in _assemblyInitializerServiceTypes.Select(ObjectContainer.Resolve).OfType<IAssemblyInitializer>())
            {
                assemblyInitializer.Initialize(assemblies);
            }
            return this;
        }
        /// <summary>Start the enode framework.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration Start()
        {
            ValidateProcessors();
            StartProcessors();
            ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name).Info("enode started...");

            return this;
        }

        #region Private Methods

        private static void ValidateSerializableTypes(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(
                    x => x.IsClass && (
                        typeof(ICommand).IsAssignableFrom(x) ||
                        typeof(IDomainEvent).IsAssignableFrom(x) ||
                        typeof(IAggregateRoot).IsAssignableFrom(x))))
                {
                    if (!type.IsSerializable)
                    {
                        throw new Exception(string.Format("{0} should be marked as serializable.", type.FullName));
                    }
                }
            }
        }
        private void ValidateProcessors()
        {
            if (_commandProcessors.Count == 0)
            {
                throw new Exception("Command processor count cannot be zero.");
            }
            if (_retryCommandProcessor == null)
            {
                throw new Exception("Retry command processor count cannot be null.");
            }
            if (_waitingCommandProcessor == null)
            {
                throw new Exception("Wating command processor count cannot be null.");
            }
            if (_uncommittedEventProcessors.Count == 0)
            {
                throw new Exception("Uncommitted event processor count cannot be zero.");
            }
            if (_committedEventProcessors.Count == 0)
            {
                throw new Exception("Committed event processor count cannot be zero.");
            }
        }
        private void StartProcessors()
        {
            foreach (var commandProcessor in _commandProcessors)
            {
                commandProcessor.Start();
            }
            _retryCommandProcessor.Start();
            _waitingCommandProcessor.Start();
            foreach (var eventProcessor in _uncommittedEventProcessors)
            {
                eventProcessor.Start();
            }
            foreach (var eventProcessor in _committedEventProcessors)
            {
                eventProcessor.Start();
            }
        }

        private static LifeStyle ParseLife(Type type)
        {
            var componentAttributes = type.GetCustomAttributes(typeof(ComponentAttribute), false);
            return !componentAttributes.Any() ? LifeStyle.Transient : ((ComponentAttribute) componentAttributes[0]).LifeStyle;
        }
        private static bool IsAssemblyInitializer<T>()
        {
            return IsAssemblyInitializer(typeof(T));
        }
        private static bool IsAssemblyInitializer(Type type)
        {
            return type.IsClass && !type.IsAbstract && typeof(IAssemblyInitializer).IsAssignableFrom(type);
        }
        private static bool IsAssemblyInitializer(object instance)
        {
            return instance is IAssemblyInitializer;
        }

        #endregion
    }

    /// <summary>Represents an option when creating the message processors.
    /// </summary>
    public class MessageProcessorOption
    {
        /// <summary>Represents the default message processor option.
        /// </summary>
        public static readonly MessageProcessorOption Default = new MessageProcessorOption();

        /// <summary>The command executor count.
        /// </summary>
        public int CommandExecutorCount { get; set; }
        /// <summary>The retry command executor count.
        /// </summary>
        public int RetryCommandExecutorCount { get; set; }
        /// <summary>The waiing command executor count.
        /// </summary>
        public int WaitingCommandExecutorCount { get; set; }
        /// <summary>The uncommitted event executor count.
        /// </summary>
        public int UncommittedEventExecutorCount { get; set; }
        /// <summary>The committed event executor count.
        /// </summary>
        public int CommittedEventExecutorCount { get; set; }
        /// <summary>The retry command dequeue interval milliseconds.
        /// </summary>
        public int RetryCommandDequeueIntervalMilliseconds { get; set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandExecutorCount"></param>
        /// <param name="retryCommandExecutorCount"></param>
        /// <param name="waitingCommandExecutorCount"></param>
        /// <param name="uncommittedEventExecutorCount"></param>
        /// <param name="committedEventExecutorCount"></param>
        /// <param name="retryCommandDequeueIntervalMilliseconds"></param>
        public MessageProcessorOption(
            int commandExecutorCount = 1,
            int retryCommandExecutorCount = 1,
            int waitingCommandExecutorCount = 1,
            int uncommittedEventExecutorCount = 1,
            int committedEventExecutorCount = 1,
            int retryCommandDequeueIntervalMilliseconds = 50)
        {
            CommandExecutorCount = commandExecutorCount;
            RetryCommandExecutorCount = retryCommandExecutorCount;
            WaitingCommandExecutorCount = waitingCommandExecutorCount;
            UncommittedEventExecutorCount = uncommittedEventExecutorCount;
            CommittedEventExecutorCount = committedEventExecutorCount;
            RetryCommandDequeueIntervalMilliseconds = retryCommandDequeueIntervalMilliseconds;
        }
    }
}