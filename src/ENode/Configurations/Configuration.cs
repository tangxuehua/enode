using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Commanding.Impl;
using ENode.Domain;
using ENode.Domain.Impl;
using ENode.Eventing;
using ENode.Eventing.Impl;
using ENode.Eventing.Impl.InMemory;
using ENode.Eventing.Impl.SQL;
using ENode.Exceptions;
using ENode.Exceptions.Impl;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;
using ENode.Infrastructure.Impl.InMemory;
using ENode.Infrastructure.Impl.SQL;
using ENode.Messaging;
using ENode.Messaging.Impl;
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

        /// <summary>Get the current setting information.
        /// </summary>
        public ConfigurationSetting Setting { get; private set; }

        /// <summary>Provides the singleton access instance.
        /// </summary>
        public static ENodeConfiguration Instance { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        private ENodeConfiguration(Configuration configuration, ConfigurationSetting setting)
        {
            Ensure.NotNull(configuration, "configuration");
            Ensure.NotNull(setting, "setting");
            _assemblyInitializerServiceTypes = new List<Type>();
            _configuration = configuration;
            Setting = setting;
        }

        /// <summary>Create the enode configuration instance.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ENodeConfiguration CreateENode(Configuration configuration, ConfigurationSetting setting)
        {
            if (Instance != null)
            {
                throw new Exception(string.Format("Could not create enode configuration instance twice."));
            }
            Instance = new ENodeConfiguration(configuration, setting);
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
            _configuration.SetDefault<IOHelper, IOHelper>();

            _configuration.SetDefault<ITypeCodeProvider, DefaultTypeCodeProvider>();
            _configuration.SetDefault<IHandlerProvider, DefaultHandlerProvider>();

            _configuration.SetDefault<IAggregateRootInternalHandlerProvider, DefaultAggregateRootInternalHandlerProvider>();
            _configuration.SetDefault<IAggregateRootFactory, DefaultAggregateRootFactory>();
            _configuration.SetDefault<IMemoryCache, DefaultMemoryCache>();
            _configuration.SetDefault<IAggregateStorage, EventSourcingAggregateStorage>();
            _configuration.SetDefault<IRepository, EventSourcingRepository>();

            _configuration.SetDefault<ISnapshotter, DefaultSnapshotter>();
            _configuration.SetDefault<ISnapshotPolicy, NoSnapshotPolicy>();
            _configuration.SetDefault<ISnapshotStore, EmptySnapshotStore>();

            _configuration.SetDefault<ICommandHandlerProvider, DefaultCommandHandlerProvider>();
            _configuration.SetDefault<ICommandStore, InMemoryCommandStore>();
            _configuration.SetDefault<ICommandExecutor, DefaultCommandExecutor>();
            _configuration.SetDefault<ICommandScheduler, DefaultCommandScheduler>();
            _configuration.SetDefault<ICommandProcessor, DefaultCommandProcessor>();
            _configuration.SetDefault<ICommandRoutingKeyProvider, DefaultCommandRoutingKeyProvider>();
            _configuration.SetDefault<ICommandService, NotImplementedCommandService>();

            _configuration.SetDefault<IEventSerializer, DefaultEventSerializer>();
            _configuration.SetDefault<IEventStore, InMemoryEventStore>();
            _configuration.SetDefault<IAggregatePublishVersionStore, InMemoryAggregatePublishVersionStore>();
            _configuration.SetDefault<IMessageHandleRecordStore, InMemoryMessageHandleRecordStore>();
            _configuration.SetDefault<IEventService, DefaultEventService>();
            _configuration.SetDefault<IDispatcher<IEvent>, DefaultEventDispatcher>();
            _configuration.SetDefault<IProcessor<IEvent>, DefaultEventProcessor>();
            _configuration.SetDefault<IProcessor<EventStream>, DefaultEventStreamProcessor>();
            _configuration.SetDefault<IProcessor<DomainEventStream>, DefaultDomainEventStreamProcessor>();
            _configuration.SetDefault<IPublisher<IEvent>, DoNothingPublisher>();
            _configuration.SetDefault<IPublisher<EventStream>, DoNothingPublisher>();
            _configuration.SetDefault<IPublisher<DomainEventStream>, DoNothingPublisher>();

            _configuration.SetDefault<IDispatcher<IPublishableException>, DefaultExceptionDispatcher>();
            _configuration.SetDefault<IProcessor<IPublishableException>, DefaultExceptionProcessor>();
            _configuration.SetDefault<IPublisher<IPublishableException>, DoNothingPublisher>();

            _configuration.SetDefault<IDispatcher<IMessage>, DefaultMessageDispatcher>();
            _configuration.SetDefault<IProcessor<IMessage>, DefaultMessageProcessor>();
            _configuration.SetDefault<IPublisher<IMessage>, DoNothingPublisher>();

            _assemblyInitializerServiceTypes.Add(typeof(IAggregateRootInternalHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(ICommandHandlerProvider));

            return this;
        }
        /// <summary>Register all the business components from the given assemblies.
        /// </summary>
        public ENodeConfiguration RegisterBusinessComponents(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(ENode.Infrastructure.TypeUtils.IsComponent))
                {
                    var life = ParseComponentLife(type);
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

        /// <summary>Use the SqlServerLockService as the ILockService.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerLockService()
        {
            _configuration.SetDefault<ILockService, SqlServerLockService>();
            return this;
        }
        /// <summary>Use the SqlServerCommandStore as the ICommandStore.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerCommandStore()
        {
            _configuration.SetDefault<ICommandStore, SqlServerCommandStore>();
            return this;
        }
        /// <summary>Use the SqlServerEventStore as the IEventStore.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerEventStore()
        {
            _configuration.SetDefault<IEventStore, SqlServerEventStore>();
            return this;
        }
        /// <summary>Use the SqlServerAggregatePublishVersionStore as the IAggregatePublishVersionStore.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerAggregatePublishVersionStore()
        {
            _configuration.SetDefault<IAggregatePublishVersionStore, SqlServerAggregatePublishVersionStore>();
            return this;
        }
        /// <summary>Use the SqlServerMessageHandleRecordStore as the IMessageHandleRecordStore.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerMessageHandleRecordStore()
        {
            _configuration.SetDefault<IMessageHandleRecordStore, SqlServerMessageHandleRecordStore>();
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

        /// <summary>Start ENode with default node type option.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration StartENode()
        {
            return StartENode(NodeType.All);
        }
        /// <summary>Start ENode with node type option.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration StartENode(NodeType nodeType)
        {
            if (nodeType == NodeType.All)
            {
                ObjectContainer.Resolve<IEventService>().Start();
                ObjectContainer.Resolve<IProcessor<DomainEventStream>>().Start();
                ObjectContainer.Resolve<IProcessor<EventStream>>().Start();
                ObjectContainer.Resolve<IProcessor<IEvent>>().Start();
                ObjectContainer.Resolve<IProcessor<IPublishableException>>().Start();
                ObjectContainer.Resolve<IProcessor<IMessage>>().Start();
                return this;
            }
            if (((int)NodeType.CommandProcessor & (int)nodeType) == (int)NodeType.CommandProcessor)
            {
                ObjectContainer.Resolve<IEventService>().Start();
            }
            if (((int)NodeType.EventProcessor & (int)nodeType) == (int)NodeType.EventProcessor)
            {
                ObjectContainer.Resolve<IProcessor<DomainEventStream>>().Start();
                ObjectContainer.Resolve<IProcessor<EventStream>>().Start();
                ObjectContainer.Resolve<IProcessor<IEvent>>().Start();
            }
            if (((int)NodeType.ExceptionProcessor & (int)nodeType) == (int)NodeType.ExceptionProcessor)
            {
                ObjectContainer.Resolve<IProcessor<IPublishableException>>().Start();
            }
            if (((int)NodeType.MessageProcessor & (int)nodeType) == (int)NodeType.MessageProcessor)
            {
                ObjectContainer.Resolve<IProcessor<IMessage>>().Start();
            }
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
                        typeof(IEvent).IsAssignableFrom(x) ||
                        typeof(IMessage).IsAssignableFrom(x) ||
                        typeof(IAggregateRoot).IsAssignableFrom(x))))
                {
                    if (!type.IsSerializable)
                    {
                        throw new Exception(string.Format("{0} should be marked as serializable.", type.FullName));
                    }
                }
            }
        }
        private static LifeStyle ParseComponentLife(Type type)
        {
            return ((ComponentAttribute)type.GetCustomAttributes(typeof(ComponentAttribute), false)[0]).LifeStyle;
        }
        private static bool IsAssemblyInitializer<T>()
        {
            return IsAssemblyInitializer(typeof(T));
        }
        private static bool IsAssemblyInitializer(Type type)
        {
            return type.IsClass && !type.IsAbstract && typeof(IAssemblyInitializer).IsAssignableFrom(type);
        }

        #endregion
    }
    public enum NodeType
    {
        All = 0,
        CommandProcessor = 1,
        EventProcessor = 2,
        ExceptionProcessor = 4,
        MessageProcessor = 8
    }

    public static class ConfigurationExtensions
    {
        public static ENodeConfiguration CreateENode(this Configuration configuration, ConfigurationSetting setting = null)
        {
            return ENodeConfiguration.CreateENode(configuration, setting ?? new ConfigurationSetting());
        }
    }
}