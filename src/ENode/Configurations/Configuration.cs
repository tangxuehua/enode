using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Configurations;
using ECommon.IoC;
using ENode.Commanding;
using ENode.Commanding.Impl;
using ENode.Domain;
using ENode.Domain.Impl;
using ENode.Eventing;
using ENode.Eventing.Impl;
using ENode.Eventing.Impl.InMemory;
using ENode.Eventing.Impl.SQL;
using ENode.Infrastructure;
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

        /// <summary>Provides the singleton access instance.
        /// </summary>
        public static ENodeConfiguration Instance { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        private ENodeConfiguration(Configuration configuration)
        {
            _configuration = configuration;
            _assemblyInitializerServiceTypes = new List<Type>();
        }

        /// <summary>Create the enode configuration instance.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ENodeConfiguration CreateENode(Configuration configuration)
        {
            if (Instance != null)
            {
                throw new ENodeException("Could not create enode configuration instance twice.");
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
            _configuration.SetDefault<IDbConnectionFactory, DefaultDbConnectionFactory>();

            _configuration.SetDefault<ICommandHandlerProvider, DefaultCommandHandlerProvider>();
            _configuration.SetDefault<IAggregateRootInternalHandlerProvider, DefaultAggregateRootInternalHandlerProvider>();
            _configuration.SetDefault<IEventHandlerProvider, DefaultEventHandlerProvider>();
            _configuration.SetDefault<IEventSynchronizerProvider, DefaultEventSynchronizerProvider>();
            _configuration.SetDefault<IAggregateRootTypeCodeProvider, NotImplementedAggregateRootTypeCodeProvider>();
            _configuration.SetDefault<IEventTypeCodeProvider, NotImplementedEventTypeCodeProvider>();
            _configuration.SetDefault<IEventHandlerTypeCodeProvider, NotImplementedEventHandlerTypeCodeProvider>();

            _configuration.SetDefault<IEventSourcingService, DefaultEventSourcingService>();
            _configuration.SetDefault<IAggregateRootFactory, DefaultAggregateRootFactory>();
            _configuration.SetDefault<IMemoryCache, DefaultMemoryCache>();
            _configuration.SetDefault<IAggregateStorage, EventSourcingAggregateStorage>();
            _configuration.SetDefault<IRepository, EventSourcingRepository>();

            _configuration.SetDefault<ISnapshotter, DefaultSnapshotter>();
            _configuration.SetDefault<ISnapshotPolicy, NoSnapshotPolicy>();
            _configuration.SetDefault<ISnapshotStore, EmptySnapshotStore>();

            _configuration.SetDefault<IWaitingCommandCache, DefaultWaitingCommandCache>();
            _configuration.SetDefault<IWaitingCommandService, DefaultWaitingCommandService>();
            _configuration.SetDefault<IRetryCommandService, DefaultRetryCommandService>();
            _configuration.SetDefault<ICommandExecutor, DefaultCommandExecutor>();
            _configuration.SetDefault<ICommandService, NotImplementedCommandService>();
            _configuration.SetDefault<IEventStreamConvertService, DefaultEventStreamConvertService>();

            _configuration.SetDefault<IEventStore, InMemoryEventStore>();
            _configuration.SetDefault<IEventPublishInfoStore, InMemoryEventPublishInfoStore>();
            _configuration.SetDefault<IEventHandleInfoStore, InMemoryEventHandleInfoStore>();
            _configuration.SetDefault<IEventHandleInfoCache, InMemoryEventHandleInfoCache>();
            _configuration.SetDefault<ICommitEventService, DefaultCommitEventService>();
            _configuration.SetDefault<IEventProcessor, DefaultEventProcessor>();
            _configuration.SetDefault<IEventPublisher, NotImplementedEventPublisher>();

            _assemblyInitializerServiceTypes.Add(typeof(ICommandHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IAggregateRootInternalHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IEventSynchronizerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IEventHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IEventSourcingService));

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

        public ENodeConfiguration UseSqlServerEventStore(string connectionString)
        {
            return UseSqlServerEventStore(connectionString, "Events", "IX_Events_CommitIndex", "IX_Events_VersonIndex");
        }
        public ENodeConfiguration UseSqlServerEventStore(string connectionString, string eventTable, string commitIndexName, string versionIndexName)
        {
            _configuration.SetDefault<IEventStore, SqlServerEventStore>(new SqlServerEventStore(connectionString, eventTable, commitIndexName, versionIndexName));
            return this;
        }
        public ENodeConfiguration UseSqlServerEventPublishInfoStore(string connectionString)
        {
            return UseSqlServerEventPublishInfoStore(connectionString, "EventPublishInfo");
        }
        public ENodeConfiguration UseSqlServerEventPublishInfoStore(string connectionString, string eventPublishInfoTable)
        {
            _configuration.SetDefault<IEventPublishInfoStore, SqlServerEventPublishInfoStore>(new SqlServerEventPublishInfoStore(connectionString, eventPublishInfoTable));
            return this;
        }
        public ENodeConfiguration UseSqlServerEventHandleInfoStore(string connectionString)
        {
            return UseSqlServerEventHandleInfoStore(connectionString, "EventHandleInfo");
        }
        public ENodeConfiguration UseSqlServerEventHandleInfoStore(string connectionString, string eventHandleInfoTable)
        {
            _configuration.SetDefault<IEventHandleInfoStore, SqlServerEventHandleInfoStore>(new SqlServerEventHandleInfoStore(connectionString, eventHandleInfoTable));
            return this;
        }
        public ENodeConfiguration UseDefaultSqlQueryDbConnectionFactory(string connectionString)
        {
            _configuration.SetDefault<ISqlQueryDbConnectionFactory, DefaultSqlQueryDbConnectionFactory>(new DefaultSqlQueryDbConnectionFactory(connectionString));
            return this;
        }

        /// <summary>Initialize all the assembly initializers with the given business assemblies.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration InitializeBusinessAssemblies(params Assembly[] assemblies)
        {
            ValidateSerializableTypes(assemblies);
            foreach (var assemblyInitializer in _assemblyInitializerServiceTypes.Select(ObjectContainer.Resolve).OfType<IAssemblyInitializer>())
            {
                assemblyInitializer.Initialize(assemblies);
            }
            return this;
        }

        /// <summary>Start the retry command service.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration StartRetryCommandService()
        {
            ObjectContainer.Resolve<IRetryCommandService>().Start();
            return this;
        }
        /// <summary>Start the waiting command service.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration StartWaitingCommandService()
        {
            ObjectContainer.Resolve<IWaitingCommandService>().Start();
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
                        throw new ENodeException("{0} should be marked as serializable.", type.FullName);
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