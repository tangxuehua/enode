using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using ENode.Infrastructure.Retring;
using ENode.Infrastructure.Serializing;
using ENode.Infrastructure.Sql;
using ENode.Messaging;
using ENode.Messaging.Impl;
using ENode.Messaging.Impl.SQL;
using ENode.Snapshoting;
using ENode.Snapshoting.Impl;

namespace ENode
{
    /// <summary>ENode framework global configuration entry point.
    /// </summary>
    public class Configuration
    {
        #region Private Vairables

        private readonly IList<Type> _assemblyInitializerServiceTypes;
        private readonly IList<ICommandProcessor> _commandProcessors;
        private ICommandProcessor _retryCommandProcessor;
        private readonly IList<IUncommittedEventProcessor> _uncommittedEventProcessors;
        private readonly IList<ICommittedEventProcessor> _committedEventProcessors;

        #endregion

        /// <summary>The single access point of the configuration.
        /// </summary>
        public static Configuration Instance { get; private set; }

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

        /// <summary>Private constructor, for implementation of singleton pattern.
        /// </summary>
        private Configuration()
        {
            _assemblyInitializerServiceTypes = new List<Type>();
            _commandProcessors = new List<ICommandProcessor>();
            _uncommittedEventProcessors = new List<IUncommittedEventProcessor>();
            _committedEventProcessors = new List<ICommittedEventProcessor>();
        }

        /// <summary>Create a new instance of configuration.
        /// </summary>
        /// <returns></returns>
        public static Configuration Create()
        {
            if (Instance != null)
            {
                throw new Exception("Could not create configuration instance twice.");
            }
            Instance = new Configuration();
            return Instance;
        }

        /// <summary>Register a implementer type as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="life">The life cycle of the implementer type.</param>
        public Configuration Register<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton)
            where TService : class
            where TImplementer : class, TService
        {
            ObjectContainer.Register<TService, TImplementer>(life);
            if (IsAssemblyInitializer<TImplementer>())
            {
                _assemblyInitializerServiceTypes.Add(typeof(TService));
            }
            return this;
        }
        /// <summary>Set the default service instance.
        /// <remarks>
        /// The life cycle of the instance is singleton.
        /// </remarks>
        /// </summary>
        public Configuration SetDefault<TService, TImplementer>(TImplementer instance)
            where TService : class
            where TImplementer : class, TService
        {
            ObjectContainer.RegisterInstance<TService, TImplementer>(instance);
            if (IsAssemblyInitializer(instance))
            {
                _assemblyInitializerServiceTypes.Add(typeof(TService));
            }
            return this;
        }
        /// <summary>Register all the default components of enode framework.
        /// </summary>
        public Configuration RegisterFrameworkComponents()
        {
            Register<ILoggerFactory, EmptyLoggerFactory>();
            Register<IBinarySerializer, DefaultBinarySerializer>();
            Register<IDbConnectionFactory, DefaultDbConnectionFactory>();
            Register<IMessageStore, EmptyMessageStore>();

            Register<IAggregateRootTypeProvider, DefaultAggregateRootTypeProvider>();
            Register<IAggregateRootInternalHandlerProvider, DefaultAggregateRootInternalHandlerProvider>();
            Register<IAggregateRootFactory, DefaultAggregateRootFactory>();
            Register<IMemoryCache, DefaultMemoryCache>();
            Register<IRepository, EventSourcingRepository>();
            Register<IMemoryCacheRebuilder, DefaultMemoryCacheRebuilder>();

            Register<ISnapshotter, DefaultSnapshotter>();
            Register<ISnapshotPolicy, NoSnapshotPolicy>();
            Register<ISnapshotStore, EmptySnapshotStore>();

            Register<ICommandHandlerProvider, DefaultCommandHandlerProvider>();
            Register<ICommandQueueRouter, DefaultCommandQueueRouter>();
            Register<IProcessingCommandCache, DefaultProcessingCommandCache>();
            Register<ICommandAsyncResultManager, DefaultCommandAsyncResultManager>();
            Register<ICommandService, DefaultCommandService>();
            Register<IRetryCommandService, DefaultRetryCommandService>();

            Register<IEventHandlerProvider, DefaultEventHandlerProvider>();
            Register<IEventSynchronizerProvider, DefaultEventSynchronizerProvider>();
            Register<IEventStore, InMemoryEventStore>();
            Register<IEventPublishInfoStore, InMemoryEventPublishInfoStore>();
            Register<IEventHandleInfoStore, InMemoryEventHandleInfoStore>();
            Register<IEventHandleInfoCache, InMemoryEventHandleInfoCache>();
            Register<IUncommittedEventQueueRouter, DefaultUncommittedEventQueueRouter>();
            Register<ICommittedEventQueueRouter, DefaultCommittedEventQueueRouter>();
            Register<IEventTableNameProvider, AggregatePerEventTableNameProvider>();
            Register<IEventSender, DefaultEventSender>();
            Register<IEventPublisher, DefaultEventPublisher>();

            Register<IRetryService, DefaultRetryService>(LifeStyle.Transient);
            Register<ICommandContext, DefaultCommandContext>(LifeStyle.Transient);
            Register<ICommandExecutor, DefaultCommandExecutor>(LifeStyle.Transient);
            Register<IUncommittedEventExecutor, DefaultUncommittedEventExecutor>(LifeStyle.Transient);
            Register<ICommittedEventExecutor, DefaultCommittedEventExecutor>(LifeStyle.Transient);

            return this;
        }
        /// <summary>Register all the business components from the given assemblies.
        /// </summary>
        public Configuration RegisterBusinessComponents(params Assembly[] assemblies)
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
        public Configuration UseSql(string connectionString)
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
        public Configuration UseSql(string connectionString, string eventTable, string queueNameFormat, string eventPublishInfoTable, string eventHandleInfoTable)
        {
            SetDefault<IEventTableNameProvider, DefaultEventTableNameProvider>(new DefaultEventTableNameProvider(eventTable));
            SetDefault<IQueueTableNameProvider, DefaultQueueTableNameProvider>(new DefaultQueueTableNameProvider(queueNameFormat));
            SetDefault<IMessageStore, SqlMessageStore>(new SqlMessageStore(connectionString));
            SetDefault<IEventStore, SqlEventStore>(new SqlEventStore(connectionString));
            SetDefault<IEventPublishInfoStore, SqlEventPublishInfoStore>(new SqlEventPublishInfoStore(connectionString, eventPublishInfoTable));
            SetDefault<IEventHandleInfoStore, SqlEventHandleInfoStore>(new SqlEventHandleInfoStore(connectionString, eventHandleInfoTable));
            return this;
        }
        /// <summary>Use the default sql querydb connection factory.
        /// </summary>
        /// <param name="connectionString">The connection string of the SQL DB.</param>
        /// <returns></returns>
        public Configuration UseDefaultSqlQueryDbConnectionFactory(string connectionString)
        {
            SetDefault<ISqlQueryDbConnectionFactory, DefaultSqlQueryDbConnectionFactory>(new DefaultSqlQueryDbConnectionFactory(connectionString));
            return this;
        }

        /// <summary>Add a command processor.
        /// </summary>
        /// <param name="commandProcessor"></param>
        /// <returns></returns>
        public Configuration AddCommandProcessor(ICommandProcessor commandProcessor)
        {
            _commandProcessors.Add(commandProcessor);
            return this;
        }
        /// <summary>Set the command processor to process the retried command.
        /// </summary>
        /// <param name="commandProcessor"></param>
        /// <returns></returns>
        public Configuration SetRetryCommandProcessor(ICommandProcessor commandProcessor)
        {
            _retryCommandProcessor = commandProcessor;
            return this;
        }
        /// <summary>Add an uncommitted event processor.
        /// </summary>
        /// <param name="eventProcessor"></param>
        /// <returns></returns>
        public Configuration AddUncommittedEventProcessor(IUncommittedEventProcessor eventProcessor)
        {
            _uncommittedEventProcessors.Add(eventProcessor);
            return this;
        }
        /// <summary>Add a committed event processor.
        /// </summary>
        /// <param name="eventProcessor"></param>
        /// <returns></returns>
        public Configuration AddCommittedEventProcessor(ICommittedEventProcessor eventProcessor)
        {
            _committedEventProcessors.Add(eventProcessor);
            return this;
        }
        /// <summary>Create all the message processors with the default queue names at once.
        /// </summary>
        /// <returns></returns>
        public Configuration CreateAllDefaultProcessors()
        {
            return CreateAllDefaultProcessors(
                new string[] { "CommandQueue" },
                "RetryCommandQueue",
                new string[] { "UncommittedEventQueue" },
                new string[] { "CommittedEventQueue" });
        }

        /// <summary>Create all the message processors with the given queue names at once.
        /// </summary>
        /// <param name="commandQueueNames">Represents the command queue names.</param>
        /// <param name="retryCommandQueueName">Represents the retry command queue name.</param>
        /// <param name="uncommittedEventQueueNames">Represents the uncommitted event queue names.</param>
        /// <param name="committedEventQueueNames">Represents the committed event queue names.</param>
        /// <param name="option">The message processor creation option.</param>
        /// <returns></returns>
        public Configuration CreateAllDefaultProcessors(IEnumerable<string> commandQueueNames, string retryCommandQueueName, IEnumerable<string> uncommittedEventQueueNames, IEnumerable<string> committedEventQueueNames, MessageProcessorOption option = null)
        {
            var messageProcessorOption = option ?? MessageProcessorOption.Default;

            foreach (var queueName in commandQueueNames)
            {
                _commandProcessors.Add(new DefaultCommandProcessor(new DefaultCommandQueue(queueName), messageProcessorOption.CommandExecutorCount));
            }
            _retryCommandProcessor = new DefaultCommandProcessor(new DefaultCommandQueue(retryCommandQueueName), messageProcessorOption.RetryCommandExecutorCount);
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
        public Configuration Initialize(params Assembly[] assemblies)
        {
            ValidateMessages(assemblies);
            foreach (var assemblyInitializer in _assemblyInitializerServiceTypes.Select(ObjectContainer.Resolve).OfType<IAssemblyInitializer>())
            {
                assemblyInitializer.Initialize(assemblies);
            }
            return this;
        }
        /// <summary>Start the enode framework.
        /// </summary>
        /// <returns></returns>
        public Configuration Start()
        {
            ValidateProcessors();
            InitializeProcessors();
            StartProcessors();
            ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name).Info("enode framework started...");

            return this;
        }

        #region Private Methods

        private static void ValidateMessages(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(x => x.IsClass && typeof(IMessage).IsAssignableFrom(x)))
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
            if (_uncommittedEventProcessors.Count == 0)
            {
                throw new Exception("Uncommitted event processor count cannot be zero.");
            }
            if (_committedEventProcessors.Count == 0)
            {
                throw new Exception("Committed event processor count cannot be zero.");
            }
        }
        private void InitializeProcessors()
        {
            foreach (var commandProcessor in _commandProcessors)
            {
                commandProcessor.Initialize();
            }
            _retryCommandProcessor.Initialize();
            foreach (var eventProcessor in _uncommittedEventProcessors)
            {
                eventProcessor.Initialize();
            }
            foreach (var eventProcessor in _committedEventProcessors)
            {
                eventProcessor.Initialize();
            }
        }
        private void StartProcessors()
        {
            foreach (var commandProcessor in _commandProcessors)
            {
                commandProcessor.Start();
            }
            _retryCommandProcessor.Start();
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
        /// <summary>The uncommitted event executor count.
        /// </summary>
        public int UncommittedEventExecutorCount { get; set; }
        /// <summary>The committed event executor count.
        /// </summary>
        public int CommittedEventExecutorCount { get; set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandExecutorCount"></param>
        /// <param name="retryCommandExecutorCount"></param>
        /// <param name="uncommittedEventExecutorCount"></param>
        /// <param name="committedEventExecutorCount"></param>
        public MessageProcessorOption(int commandExecutorCount = 1, int retryCommandExecutorCount = 1, int uncommittedEventExecutorCount = 1, int committedEventExecutorCount = 1)
        {
            CommandExecutorCount = commandExecutorCount;
            RetryCommandExecutorCount = retryCommandExecutorCount;
            UncommittedEventExecutorCount = uncommittedEventExecutorCount;
            CommittedEventExecutorCount = committedEventExecutorCount;
        }
    }
}