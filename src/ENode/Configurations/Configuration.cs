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

        #endregion

        public static ENodeConfiguration Instance { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        private ENodeConfiguration(Configuration configuration)
        {
            _configuration = configuration;
            _assemblyInitializerServiceTypes = new List<Type>();
        }

        public static ENodeConfiguration CreateENode(Configuration configuration)
        {
            if (Instance != null)
            {
                throw new Exception("Could not create enode configuration instance twice.");
            }
            Instance = new ENodeConfiguration(configuration);
            return Instance;
        }

        /// <summary>Get the ecommon configuration.
        /// </summary>
        /// <returns></returns>
        public Configuration GetCommonConfiguration()
        {
            return _configuration;
        }
        /// <summary>Register all the default components of enode framework.
        /// </summary>
        public ENodeConfiguration RegisterENodeComponents()
        {
            _configuration.SetDefault<ILoggerFactory, EmptyLoggerFactory>();
            _configuration.SetDefault<IBinarySerializer, DefaultBinarySerializer>();
            _configuration.SetDefault<IDbConnectionFactory, DefaultDbConnectionFactory>();

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
            _configuration.SetDefault<IProcessingCommandCache, DefaultProcessingCommandCache>();
            _configuration.SetDefault<IWaitingCommandCache, DefaultWaitingCommandCache>();
            _configuration.SetDefault<IWaitingCommandService, DefaultWaitingCommandService>();
            _configuration.SetDefault<ICommandCompletionEventManager, DefaultCommandCompletionEventManager>();
            _configuration.SetDefault<IRetryCommandService, DefaultRetryCommandService>();

            _configuration.SetDefault<IEventHandlerProvider, DefaultEventHandlerProvider>();
            _configuration.SetDefault<IEventSynchronizerProvider, DefaultEventSynchronizerProvider>();
            _configuration.SetDefault<IEventStore, InMemoryEventStore>();
            _configuration.SetDefault<IEventPublishInfoStore, InMemoryEventPublishInfoStore>();
            _configuration.SetDefault<IEventHandleInfoStore, InMemoryEventHandleInfoStore>();
            _configuration.SetDefault<IEventHandleInfoCache, InMemoryEventHandleInfoCache>();
            _configuration.SetDefault<IEventTableNameProvider, AggregatePerEventTableNameProvider>();
            _configuration.SetDefault<IPublishEventService, DefaultPublishEventService>();
            _configuration.SetDefault<IEventProcessor, DefaultEventProcessor>();

            _configuration.SetDefault<IActionExecutionService, DefaultActionExecutionService>(LifeStyle.Transient);

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
            ObjectContainer.Resolve<IRetryCommandService>().Start();
            ObjectContainer.Resolve<IWaitingCommandService>().Start();
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

    public static class ConfigurationExtensions
    {
        public static ENodeConfiguration CreateENode(this Configuration configuration)
        {
            return ENodeConfiguration.CreateENode(configuration);
        }
    }
}