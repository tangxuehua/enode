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
using ENode.ThirdParty;

namespace ENode
{
    /// <summary>enode framework global configuration entry.
    /// </summary>
    public class Configuration
    {
        private static Configuration _instance;
        private IList<ICommandProcessor> _commandProcessors;
        private ICommandProcessor _retryCommandProcessor;
        private IList<IEventProcessor> _eventProcessors;

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
        /// <summary>Get all the event queues.
        /// </summary>
        public IEnumerable<IEventQueue> GetEventQueues()
        {
            return _eventProcessors.Select(x => x.BindingQueue);
        }

        private Configuration()
        {
            _commandProcessors = new List<ICommandProcessor>();
            _eventProcessors = new List<IEventProcessor>();
        }

        /// <summary>Create an instance of configuration.
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

        /// <summary>Use the open source Tiny IoC container as the default object container.
        /// <remarks>
        /// Register all the default service implementation types of the framework into the container.
        /// </remarks>
        /// </summary>
        public Configuration UseTinyObjectContainer()
        {
            ObjectContainer.SetCurrentContainer(new TinyObjectContainer());
            RegisterAllDefaultFrameworkComponents();
            return this;
        }
        /// <summary>Use the Log4NetLoggerFactory as ILoggerFactory.
        /// </summary>
        /// <returns></returns>
        public Configuration UseLog4Net(string configFile)
        {
            ObjectContainer.Register<ILoggerFactory>(new Log4NetLoggerFactory(configFile));
            return this;
        }
        /// <summary>Use an efficient open source binary serializer.
        /// </summary>
        /// <returns></returns>
        public Configuration UseNetBinarySerializer(params Assembly[] assemblies)
        {
            ObjectContainer.Register<IBinarySerializer>(new NetBinarySerializer(assemblies));
            return this;
        }

        /// <summary>Register all the service types in the assemblies.
        /// </summary>
        /// <returns></returns>
        public Configuration RegisterServiceTypes(params Assembly[] assemblies)
        {
            ObjectContainer.RegisterTypes(TypeUtils.IsService, assemblies);
            return this;
        }
        /// <summary>Register all the component types in the assemblies.
        /// </summary>
        /// <returns></returns>
        public Configuration RegisterComponentTypes(params Assembly[] assemblies)
        {
            ObjectContainer.RegisterTypes(TypeUtils.IsComponent, assemblies);
            return this;
        }
        /// <summary>Register all the repository types in the assemblies.
        /// </summary>
        /// <returns></returns>
        public Configuration RegisterRepositoryTypes(params Assembly[] assemblies)
        {
            ObjectContainer.RegisterTypes(TypeUtils.IsRepository, assemblies);
            return this;
        }
        /// <summary>Register all the event handler types in the assemblies.
        /// </summary>
        /// <returns></returns>
        public Configuration RegisterEventHandlerTypes(params Assembly[] assemblies)
        {
            ObjectContainer.RegisterTypes(TypeUtils.IsEventHandler, assemblies);
            return this;
        }

        /// <summary>Use the DefaultCommandHandlerProvider as ICommandHandlerProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultCommandHandlerProvider(params Assembly[] assemblies)
        {
            ObjectContainer.Register<ICommandHandlerProvider>(new DefaultCommandHandlerProvider(assemblies));
            return this;
        }
        /// <summary>Use the DefaultAggregateRootInternalHandlerProvider as IAggregateRootInternalHandlerProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultAggregateRootInternalHandlerProvider(params Assembly[] assemblies)
        {
            ObjectContainer.Register<IAggregateRootInternalHandlerProvider>(new DefaultAggregateRootInternalHandlerProvider(assemblies));
            return this;
        }
        /// <summary>Use the DefaultAggregateRootTypeProvider as IAggregateRootTypeProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultAggregateRootTypeProvider(params Assembly[] assemblies)
        {
            ObjectContainer.Register<IAggregateRootTypeProvider>(new DefaultAggregateRootTypeProvider(assemblies));
            return this;
        }
        /// <summary>Use the DefaultEventHandlerProvider as IEventHandlerProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultEventHandlerProvider(params Assembly[] assemblies)
        {
            ObjectContainer.Register<IEventHandlerProvider>(new DefaultEventHandlerProvider(assemblies));
            return this;
        }

        /// <summary>Use the RedisMemoryCache as IMemoryCache.
        /// </summary>
        /// <returns></returns>
        public Configuration UseRedisMemoryCache(string host, int port)
        {
            ObjectContainer.Register<IMemoryCache>(new RedisMemoryCache(host, port));
            return this;
        }

        #region SQL Config

        /// <summary>Use the SqlEventStore as IEventStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseSqlEventStore(string connectionString)
        {
            ObjectContainer.Register<IEventStore>(new SqlEventStore(connectionString));
            return this;
        }
        /// <summary>Use the SqlMessageStore as IMessageStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseSqlMessageStore(string connectionString)
        {
            ObjectContainer.Register<IMessageStore>(new SqlMessageStore(connectionString));
            return this;
        }
        /// <summary>Use the SqlEventPublishInfoStore as IEventPublishInfoStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseSqlEventPublishInfoStore(string connectionString, string tableName)
        {
            ObjectContainer.Register<IEventPublishInfoStore>(new SqlEventPublishInfoStore(connectionString, tableName));
            return this;
        }
        /// <summary>Use the SqlEventHandleInfoStore as IEventHandleInfoStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseSqlEventHandleInfoStore(string connectionString, string tableName)
        {
            ObjectContainer.Register<IEventHandleInfoStore>(new SqlEventHandleInfoStore(connectionString, tableName));
            return this;
        }
        /// <summary>Use the DefaultEventTableNameProvider as IEventTableNameProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultEventTableNameProvider(string tableName)
        {
            ObjectContainer.Register<IEventTableNameProvider>(new DefaultEventTableNameProvider(tableName));
            return this;
        }
        /// <summary>Use the AggregatePerEventTableNameProvider as IEventTableNameProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseAggregatePerEventTableNameProvider(string tableName)
        {
            ObjectContainer.Register<IEventTableNameProvider, AggregatePerEventTableNameProvider>();
            return this;
        }
        /// <summary>Use the DefaultQueueTableNameProvider as IQueueTableNameProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultQueueTableNameProvider(string tableNameFormat = null)
        {
            ObjectContainer.Register<IQueueTableNameProvider>(new DefaultQueueTableNameProvider(tableNameFormat));
            return this;
        }

        #endregion

        #region MongoDB Config

        /// <summary>Use MongoEventStore as IEventStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseMongoEventStore(string connectionString)
        {
            ObjectContainer.Register<IEventStore>(new MongoEventStore(connectionString));
            return this;
        }
        /// <summary>Use MongoMessageStore as IMessageStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseMongoMessageStore(string connectionString)
        {
            ObjectContainer.Register<IMessageStore>(new MongoMessageStore(connectionString));
            return this;
        }
        /// <summary>Use the MongoEventPublishInfoStore as IEventPublishInfoStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseMongoEventPublishInfoStore(string connectionString, string collectionName)
        {
            ObjectContainer.Register<IEventPublishInfoStore>(new MongoEventPublishInfoStore(connectionString, collectionName));
            return this;
        }
        /// <summary>Use the MongoEventHandleInfoStore as IEventHandleInfoStore.
        /// </summary>
        /// <returns></returns>
        public Configuration UseMongoEventHandleInfoStore(string connectionString, string collectionName)
        {
            ObjectContainer.Register<IEventHandleInfoStore>(new MongoEventHandleInfoStore(connectionString, collectionName));
            return this;
        }
        /// <summary>Use the DefaultEventCollectionNameProvider as IMongoCollectionNameProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultEventCollectionNameProvider(string collectionName)
        {
            ObjectContainer.Register<IMongoCollectionNameProvider>(new DefaultEventCollectionNameProvider(collectionName));
            return this;
        }
        /// <summary>Use the AggregatePerEventCollectionNameProvider as IMongoCollectionNameProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseAggregatePerEventCollectionNameProvider()
        {
            ObjectContainer.Register<IMongoCollectionNameProvider, AggregatePerEventCollectionNameProvider>();
            return this;
        }
        /// <summary>Use the DefaultQueueCollectionNameProvider as IQueueCollectionNameProvider.
        /// </summary>
        /// <returns></returns>
        public Configuration UseDefaultQueueCollectionNameProvider(string collectionNameFormat = null)
        {
            ObjectContainer.Register<IQueueCollectionNameProvider>(new DefaultQueueCollectionNameProvider(collectionNameFormat));
            return this;
        }

        #endregion

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
        /// <summary>Add a event processor.
        /// </summary>
        /// <param name="eventProcessor"></param>
        /// <returns></returns>
        public Configuration AddEventProcessor(IEventProcessor eventProcessor)
        {
            _eventProcessors.Add(eventProcessor);
            return this;
        }
        /// <summary>Config all the message processors with the given queue names at once.
        /// </summary>
        /// <param name="commandQueueNames">Represents the command queue names.</param>
        /// <param name="retryCommandQueueName">Represents the retry command queue name.</param>
        /// <param name="eventQueueNames">Represents the committed event queue names.</param>
        /// <returns></returns>
        public Configuration UseAllDefaultProcessors(IEnumerable<string> commandQueueNames, string retryCommandQueueName, IEnumerable<string> eventQueueNames, MessageProcessorOption option = null)
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
            foreach (var queueName in eventQueueNames)
            {
                _eventProcessors.Add(new DefaultEventProcessor(new DefaultEventQueue(queueName), messageProcessorOption.EventExecutorCount));
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

        private void RegisterAllDefaultFrameworkComponents()
        {
            ObjectContainer.Register<ILoggerFactory, EmptyLoggerFactory>();
            ObjectContainer.Register<IJsonSerializer, NewtonsoftJsonSerializer>();
            ObjectContainer.Register<IBinarySerializer, DefaultBinarySerializer>();
            ObjectContainer.Register<IStringSerializer, DefaultStringSerializer>();
            ObjectContainer.Register<IDbConnectionFactory, SqlDbConnectionFactory>();
            ObjectContainer.Register<IMessageStore, EmptyMessageStore>();

            ObjectContainer.Register<IAggregateRootFactory, DefaultAggregateRootFactory>();
            ObjectContainer.Register<IMemoryCache, DefaultMemoryCache>();
            ObjectContainer.Register<IMemoryCacheRefreshService, DefaultMemoryCacheRefreshService>();
            ObjectContainer.Register<IRepository, EventSourcingRepository>();
            ObjectContainer.Register<IMemoryCacheRebuilder, DefaultMemoryCacheRebuilder>();

            ObjectContainer.Register<ISnapshotter, DefaultSnapshotter>();
            ObjectContainer.Register<ISnapshotPolicy, NoSnapshotPolicy>();
            ObjectContainer.Register<ISnapshotStore, EmptySnapshotStore>();

            ObjectContainer.Register<ICommandQueueRouter, DefaultCommandQueueRouter>();
            ObjectContainer.Register<IProcessingCommandCache, DefaultProcessingCommandCache>();
            ObjectContainer.Register<ICommandAsyncResultManager, DefaultCommandAsyncResultManager>();
            ObjectContainer.Register<ICommandService, DefaultCommandService>();
            ObjectContainer.Register<IRetryCommandService, DefaultRetryCommandService>();

            ObjectContainer.Register<IEventStore, InMemoryEventStore>();
            ObjectContainer.Register<IEventPublishInfoStore, InMemoryEventPublishInfoStore>();
            ObjectContainer.Register<IEventHandleInfoStore, InMemoryEventHandleInfoStore>();
            ObjectContainer.Register<IEventQueueRouter, DefaultEventQueueRouter>();
            ObjectContainer.Register<IEventTableNameProvider, AggregatePerEventTableNameProvider>();
            ObjectContainer.Register<IEventPublisher, DefaultEventPublisher>();

            ObjectContainer.Register<ICommandContext, DefaultCommandContext>(LifeStyle.Transient);
            ObjectContainer.Register<ICommandExecutor, DefaultCommandExecutor>(LifeStyle.Transient);
            ObjectContainer.Register<IEventExecutor, DefaultEventExecutor>(LifeStyle.Transient);
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
            if (_eventProcessors.Count == 0)
            {
                throw new Exception("Event processor count cannot be zero.");
            }
        }
        private void InitializeProcessors()
        {
            foreach (var commandProcessor in _commandProcessors)
            {
                commandProcessor.Initialize();
            }
            _retryCommandProcessor.Initialize();
            foreach (var eventProcessor in _eventProcessors)
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
            foreach (var eventProcessor in _eventProcessors)
            {
                eventProcessor.Start();
            }
        }
    }

    public class MessageProcessorOption
    {
        private static readonly MessageProcessorOption _default = new MessageProcessorOption();

        public int CommandExecutorCount { get; set; }
        public int RetryCommandExecutorCount { get; set; }
        public int EventExecutorCount { get; set; }

        public static MessageProcessorOption Default
        {
            get { return _default; }
        }

        public MessageProcessorOption(int commandExecutorCount = 1, int retryCommandExecutorCount = 1, int eventExecutorCount = 1)
        {
            CommandExecutorCount = commandExecutorCount;
            RetryCommandExecutorCount = retryCommandExecutorCount;
            EventExecutorCount = eventExecutorCount;
        }
    }
}