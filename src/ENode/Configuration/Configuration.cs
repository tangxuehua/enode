using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;
using ENode.Eventing.Storage.MongoDB;
using ENode.Eventing.Storage.Sql;
using ENode.Infrastructure;
using ENode.Messaging;
using ENode.Messaging.Storage.Sql;
using ENode.Messaging.Storage.MongoDB;
using ENode.Snapshoting;

namespace ENode
{
    /// <summary>ENode framework global configuration entry point.
    /// </summary>
    public class Configuration
    {
        private static Configuration _instance;
        private IList<Type> _assemblyInitializerServiceTypes;
        private IList<ICommandProcessor> _commandProcessors;
        private ICommandProcessor _retryCommandProcessor;
        private IList<IUncommittedEventProcessor> _uncommittedEventProcessors;
        private IList<ICommittedEventProcessor> _committedEventProcessors;

        /// <summary>The single access point of the configuration.
        /// </summary>
        public static Configuration Instance { get { return _instance; } }

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
            if (_instance != null)
            {
                throw new Exception("Could not create configuration instance twice.");
            }
            _instance = new Configuration();
            return _instance;
        }

        /// <summary>Use autofac as the object container.
        /// </summary>
        public Configuration UseAutofacContainer()
        {
            ObjectContainer.SetContainer(new AutofacObjectContainer());
            return this;
        }
        /// <summary>Register a implementer type as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="life">The life cycle of the implementer type.</param>
        public Configuration Register<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton) where TService : class where TImplementer : class, TService
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
        public Configuration SetDefault<TService, TImplementer>(TImplementer instance) where TService : class where TImplementer : class, TService
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
            Register<IJsonSerializer, NewtonsoftJsonSerializer>();
            Register<IBinarySerializer, DefaultBinarySerializer>();
            Register<IStringSerializer, DefaultStringSerializer>();
            Register<IDbConnectionFactory, SqlDbConnectionFactory>();
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
            Register<IEventPersistenceSynchronizerProvider, DefaultEventPersistenceSynchronizerProvider>();
            Register<IEventStore, InMemoryEventStore>();
            Register<IEventPublishInfoStore, InMemoryEventPublishInfoStore>();
            Register<IEventHandleInfoStore, InMemoryEventHandleInfoStore>();
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
                foreach (var type in assembly.GetExportedTypes().Where(TypeUtils.IsComponent))
                {
                    ObjectContainer.RegisterType(type, ParseLife(type));
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
        /// <param name="eventTable">The table used to store all the domain events.</param>
        /// <param name="queueNameFormat">The format of the queue name.</param>
        /// <param name="eventPublishInfoTable">The table used to store all the event publish information.</param>
        /// <param name="eventHandleInfoTable">The table used to store all the event handle information.</param>
        /// <returns></returns>
        public Configuration UseSqlAsStorage(string connectionString, string eventTable, string queueNameFormat, string eventPublishInfoTable, string eventHandleInfoTable)
        {
            SetDefault<IEventTableNameProvider, DefaultEventTableNameProvider>(new DefaultEventTableNameProvider(eventTable));
            SetDefault<IQueueTableNameProvider, DefaultQueueTableNameProvider>(new DefaultQueueTableNameProvider(queueNameFormat));
            SetDefault<IMessageStore, SqlMessageStore>(new SqlMessageStore(connectionString));
            SetDefault<IEventStore, SqlEventStore>(new SqlEventStore(connectionString));
            SetDefault<IEventPublishInfoStore, SqlEventPublishInfoStore>(new SqlEventPublishInfoStore(connectionString, eventPublishInfoTable));
            SetDefault<IEventHandleInfoStore, SqlEventHandleInfoStore>(new SqlEventHandleInfoStore(connectionString, eventHandleInfoTable));
            return this;
        }
        /// <summary>Use MongoDB as the storage of the whole framework.
        /// </summary>
        /// <param name="connectionString">The connection string of the mongodb server.</param>
        /// <param name="eventCollectionName">The mongo collection used to store all the domain event.</param>
        /// <param name="queueNameFormat">The format of the queue name.</param>
        /// <param name="eventPublishInfoCollectionName">The collection used to store all the event publish information.</param>
        /// <param name="eventHandleInfoCollectionName">The collection used to store all the event handle information.</param>
        /// <returns></returns>
        public Configuration UseMongoAsStorage(string connectionString, string eventCollectionName, string queueNameFormat, string eventPublishInfoCollectionName, string eventHandleInfoCollectionName)
        {
            SetDefault<IEventCollectionNameProvider, DefaultEventCollectionNameProvider>(new DefaultEventCollectionNameProvider(eventCollectionName));
            SetDefault<IQueueCollectionNameProvider, DefaultQueueCollectionNameProvider>(new DefaultQueueCollectionNameProvider(queueNameFormat));
            SetDefault<IMessageStore, MongoMessageStore>(new MongoMessageStore(connectionString));
            SetDefault<IEventStore, MongoEventStore>(new MongoEventStore(connectionString));
            SetDefault<IEventPublishInfoStore, MongoEventPublishInfoStore>(new MongoEventPublishInfoStore(connectionString, eventPublishInfoCollectionName));
            SetDefault<IEventHandleInfoStore, MongoEventHandleInfoStore>(new MongoEventHandleInfoStore(connectionString, eventHandleInfoCollectionName));
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
        /// <summary>Create all the message processors with the given queue names at once.
        /// </summary>
        /// <param name="commandQueueNames">Represents the command queue names.</param>
        /// <param name="retryCommandQueueName">Represents the retry command queue name.</param>
        /// <param name="committedEventQueueNames">Represents the committed event queue names.</param>
        /// <returns></returns>
        public Configuration CreateAllDefaultProcessors(IEnumerable<string> commandQueueNames, string retryCommandQueueName, IEnumerable<string> uncommittedEventQueueNames, IEnumerable<string> committedEventQueueNames, MessageProcessorOption option = null)
        {
            var messageProcessorOption = option;
            if (messageProcessorOption == null)
            {
                messageProcessorOption = MessageProcessorOption.Default;
            }

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
            foreach (var serviceType in _assemblyInitializerServiceTypes)
            {
                (ObjectContainer.Resolve(serviceType) as IAssemblyInitializer).Initialize(assemblies);
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
        /// <summary>Start the framework with all the default configuration.
        /// <remarks>
        /// Not use this api only when you just want to do some in-memory based testing.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public static Configuration StartWithAllDefault(params Assembly[] assemblies)
        {
            return Configuration
                .Create()
                .UseAutofacContainer()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .SetDefault<ILoggerFactory, Log4NetLoggerFactory>(new Log4NetLoggerFactory("log4net.config"))
                .CreateAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "UncommittedEventQueue" },
                    new string[] { "CommittedEventQueue" })
                .Initialize(assemblies)
                .Start();
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

        private LifeStyle ParseLife(Type type)
        {
            var componentAttributes = type.GetCustomAttributes(typeof(ComponentAttribute), false);
            return componentAttributes.Count() <= 0 ? LifeStyle.Transient : (componentAttributes[0] as ComponentAttribute).LifeStyle;
        }
        private bool IsAssemblyInitializer<T>()
        {
            return typeof(IAssemblyInitializer).IsAssignableFrom(typeof(T));
        }
        private bool IsAssemblyInitializer(Type type)
        {
            return typeof(IAssemblyInitializer).IsAssignableFrom(type);
        }
        private bool IsAssemblyInitializer(object instance)
        {
            return instance is IAssemblyInitializer;
        }
    }

    public class MessageProcessorOption
    {
        private static readonly MessageProcessorOption _default = new MessageProcessorOption();

        public int CommandExecutorCount { get; set; }
        public int RetryCommandExecutorCount { get; set; }
        public int UncommittedEventExecutorCount { get; set; }
        public int CommittedEventExecutorCount { get; set; }

        public static MessageProcessorOption Default
        {
            get { return _default; }
        }

        public MessageProcessorOption(int commandExecutorCount = 1, int retryCommandExecutorCount = 1, int uncommittedEventExecutorCount = 1, int committedEventExecutorCount = 1)
        {
            CommandExecutorCount = commandExecutorCount;
            RetryCommandExecutorCount = retryCommandExecutorCount;
            UncommittedEventExecutorCount = uncommittedEventExecutorCount;
            CommittedEventExecutorCount = committedEventExecutorCount;
        }
    }
}