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
            _configuration.SetDefault<ITypeCodeProvider<IAggregateRoot>, DefaultTypeCodeProvider<IAggregateRoot>>();
            _configuration.SetDefault<ITypeCodeProvider<ICommand>, DefaultTypeCodeProvider<ICommand>>();
            _configuration.SetDefault<ITypeCodeProvider<IEvent>, DefaultTypeCodeProvider<IEvent>>();
            _configuration.SetDefault<ITypeCodeProvider<IPublishableException>, DefaultTypeCodeProvider<IPublishableException>>();
            _configuration.SetDefault<ITypeCodeProvider<IMessage>, DefaultTypeCodeProvider<IMessage>>();
            _configuration.SetDefault<ITypeCodeProvider<IEventHandler>, DefaultTypeCodeProvider<IEventHandler>>();
            _configuration.SetDefault<ITypeCodeProvider<IExceptionHandler>, DefaultTypeCodeProvider<IExceptionHandler>>();
            _configuration.SetDefault<ITypeCodeProvider<IMessageHandler>, DefaultTypeCodeProvider<IMessageHandler>>();

            _configuration.SetDefault<IAggregateRootInternalHandlerProvider, DefaultAggregateRootInternalHandlerProvider>();
            _configuration.SetDefault<IHandlerProvider<ICommandHandler>, DefaultCommandHandlerProvider>();
            _configuration.SetDefault<IHandlerProvider<IEventHandler>, DefaultEventHandlerProvider>();
            _configuration.SetDefault<IHandlerProvider<IExceptionHandler>, DefaultExceptionHandlerProvider>();
            _configuration.SetDefault<IHandlerProvider<IMessageHandler>, DefaultMessageHandlerProvider>();

            _configuration.SetDefault<IEventSerializer, DefaultEventSerializer>();

            _configuration.SetDefault<IAggregateRootFactory, DefaultAggregateRootFactory>();
            _configuration.SetDefault<IMemoryCache, DefaultMemoryCache>();
            _configuration.SetDefault<IAggregateStorage, EventSourcingAggregateStorage>();
            _configuration.SetDefault<IRepository, EventSourcingRepository>();

            _configuration.SetDefault<ISnapshotter, DefaultSnapshotter>();
            _configuration.SetDefault<ISnapshotPolicy, NoSnapshotPolicy>();
            _configuration.SetDefault<ISnapshotStore, EmptySnapshotStore>();

            _configuration.SetDefault<ICommandStore, InMemoryCommandStore>();
            _configuration.SetDefault<IWaitingCommandService, DefaultWaitingCommandService>();
            _configuration.SetDefault<IRetryCommandService, DefaultRetryCommandService>();
            _configuration.SetDefault<IExecutedCommandService, DefaultExecutedCommandService>();
            _configuration.SetDefault<ICommandExecutor, DefaultCommandExecutor>();
            _configuration.SetDefault<ICommandRouteKeyProvider, DefaultCommandRouteKeyProvider>();
            _configuration.SetDefault<ICommandService, NotImplementedCommandService>();

            _configuration.SetDefault<IEventStore, InMemoryEventStore>();
            _configuration.SetDefault<IEventPublishInfoStore, InMemoryEventPublishInfoStore>();
            _configuration.SetDefault<IEventHandleInfoStore, InMemoryEventHandleInfoStore>();
            _configuration.SetDefault<IEventHandleInfoCache, InMemoryEventHandleInfoCache>();
            _configuration.SetDefault<IEventService, DefaultEventService>();
            _configuration.SetDefault<IProcessor<IEvent>, DefaultEventProcessor>();
            _configuration.SetDefault<IProcessor<EventStream>, DefaultEventProcessor>();
            _configuration.SetDefault<IProcessor<DomainEventStream>, DefaultEventProcessor>();
            _configuration.SetDefault<IPublisher<IEvent>, NotImplementedEventPublisher>();
            _configuration.SetDefault<IPublisher<EventStream>, NotImplementedEventPublisher>();
            _configuration.SetDefault<IPublisher<DomainEventStream>, NotImplementedEventPublisher>();

            _configuration.SetDefault<IProcessor<IPublishableException>, DefaultExceptionProcessor>();
            _configuration.SetDefault<IPublisher<IPublishableException>, NotImplementedExceptionPublisher>();

            _configuration.SetDefault<IProcessor<IMessage>, DefaultMessageProcessor>();
            _configuration.SetDefault<IPublisher<IMessage>, NotImplementedMessagePublisher>();

            _assemblyInitializerServiceTypes.Add(typeof(IAggregateRootInternalHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IHandlerProvider<ICommandHandler>));
            _assemblyInitializerServiceTypes.Add(typeof(IHandlerProvider<IEventHandler>));
            _assemblyInitializerServiceTypes.Add(typeof(IHandlerProvider<IExceptionHandler>));
            _assemblyInitializerServiceTypes.Add(typeof(IHandlerProvider<IMessageHandler>));

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
        /// <summary>Use the SqlServerEventPublishInfoStore as the IEventPublishInfoStore.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerEventPublishInfoStore()
        {
            _configuration.SetDefault<IEventPublishInfoStore, SqlServerEventPublishInfoStore>();
            return this;
        }
        /// <summary>Use the SqlServerEventHandleInfoStore as the IEventHandleInfoStore.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerEventHandleInfoStore()
        {
            _configuration.SetDefault<IEventHandleInfoStore, SqlServerEventHandleInfoStore>();
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

        /// <summary>Start ENode with node type option.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration StartENode(NodeType nodeType)
        {
            if (((int)NodeType.CommandProcessor & (int)nodeType) == (int)NodeType.CommandProcessor)
            {
                ObjectContainer.Resolve<IRetryCommandService>().Start();
                ObjectContainer.Resolve<IWaitingCommandService>().Start();
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