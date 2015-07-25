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
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;
using ENode.Infrastructure.Impl.InMemory;
using ENode.Infrastructure.Impl.SQL;
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
            _configuration.SetDefault<ITypeCodeProvider, DefaultTypeCodeProvider>();
            _configuration.SetDefault<IMessageHandlerProvider, DefaultMessageHandlerProvider>();
            _configuration.SetDefault<ITwoMessageHandlerProvider, DefaultTwoMessageHandlerProvider>();
            _configuration.SetDefault<IThreeMessageHandlerProvider, DefaultThreeMessageHandlerProvider>();

            _configuration.SetDefault<IAggregateRootInternalHandlerProvider, DefaultAggregateRootInternalHandlerProvider>();
            _configuration.SetDefault<IAggregateRootFactory, DefaultAggregateRootFactory>();
            _configuration.SetDefault<IMemoryCache, DefaultMemoryCache>();
            _configuration.SetDefault<IAggregateStorage, EventSourcingAggregateStorage>();
            _configuration.SetDefault<IRepository, EventSourcingRepository>();

            _configuration.SetDefault<ISnapshotter, DefaultSnapshotter>();
            _configuration.SetDefault<ISnapshotPolicy, NoSnapshotPolicy>();
            _configuration.SetDefault<ISnapshotStore, EmptySnapshotStore>();

            _configuration.SetDefault<ICommandAsyncHandlerProvider, DefaultCommandAsyncHandlerProvider>();
            _configuration.SetDefault<ICommandHandlerProvider, DefaultCommandHandlerProvider>();
            _configuration.SetDefault<ICommandStore, InMemoryCommandStore>();
            _configuration.SetDefault<ICommandRoutingKeyProvider, DefaultCommandRoutingKeyProvider>();
            _configuration.SetDefault<ICommandService, NotImplementedCommandService>();

            _configuration.SetDefault<IEventSerializer, DefaultEventSerializer>();
            _configuration.SetDefault<IEventStore, InMemoryEventStore>();
            _configuration.SetDefault<ISequenceMessagePublishedVersionStore, InMemorySequenceMessagePublishedVersionStore>();
            _configuration.SetDefault<IMessageHandleRecordStore, InMemoryMessageHandleRecordStore>();
            _configuration.SetDefault<IEventService, DefaultEventService>();

            _configuration.SetDefault<IMessageDispatcher, DefaultMessageDispatcher>();

            _configuration.SetDefault<IMessagePublisher<IApplicationMessage>, DoNothingPublisher>();
            _configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DoNothingPublisher>();
            _configuration.SetDefault<IMessagePublisher<IPublishableException>, DoNothingPublisher>();

            _configuration.SetDefault<IProcessingMessageHandler<ProcessingCommand, ICommand, CommandResult>, DefaultProcessingCommandHandler>();
            _configuration.SetDefault<IProcessingMessageHandler<ProcessingApplicationMessage, IApplicationMessage, bool>, DefaultProcessingMessageHandler<ProcessingApplicationMessage, IApplicationMessage, bool>>();
            _configuration.SetDefault<IProcessingMessageHandler<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>, DomainEventStreamMessageHandler>();
            _configuration.SetDefault<IProcessingMessageHandler<ProcessingPublishableExceptionMessage, IPublishableException, bool>, DefaultProcessingMessageHandler<ProcessingPublishableExceptionMessage, IPublishableException, bool>>();

            _configuration.SetDefault<IProcessingMessageScheduler<ProcessingCommand, ICommand, CommandResult>, DefaultProcessingMessageScheduler<ProcessingCommand, ICommand, CommandResult>>();
            _configuration.SetDefault<IProcessingMessageScheduler<ProcessingApplicationMessage, IApplicationMessage, bool>, DefaultProcessingMessageScheduler<ProcessingApplicationMessage, IApplicationMessage, bool>>();
            _configuration.SetDefault<IProcessingMessageScheduler<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>, DefaultProcessingMessageScheduler<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>>();
            _configuration.SetDefault<IProcessingMessageScheduler<ProcessingPublishableExceptionMessage, IPublishableException, bool>, DefaultProcessingMessageScheduler<ProcessingPublishableExceptionMessage, IPublishableException, bool>>();

            _configuration.SetDefault<IMessageProcessor<ProcessingCommand, ICommand, CommandResult>, DefaultMessageProcessor<ProcessingCommand, ICommand, CommandResult>>();
            _configuration.SetDefault<IMessageProcessor<ProcessingApplicationMessage, IApplicationMessage, bool>, DefaultMessageProcessor<ProcessingApplicationMessage, IApplicationMessage, bool>>();
            _configuration.SetDefault<IMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>, DefaultMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>>();
            _configuration.SetDefault<IMessageProcessor<ProcessingPublishableExceptionMessage, IPublishableException, bool>, DefaultMessageProcessor<ProcessingPublishableExceptionMessage, IPublishableException, bool>>();

            _assemblyInitializerServiceTypes.Add(typeof(IAggregateRootInternalHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IMessageHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(ITwoMessageHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(IThreeMessageHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(ICommandHandlerProvider));
            _assemblyInitializerServiceTypes.Add(typeof(ICommandAsyncHandlerProvider));

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
        /// <summary>Use the SqlServerSequenceMessagePublishedVersionStore as the ISequenceMessagePublishedVersionStore.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration UseSqlServerSequenceMessagePublishedVersionStore()
        {
            _configuration.SetDefault<ISequenceMessagePublishedVersionStore, SqlServerSequenceMessagePublishedVersionStore>();
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
        /// <summary>Set the concurrent level for command scheduler.
        /// </summary>
        /// <param name="concurrentLevel"></param>
        /// <returns></returns>
        public ENodeConfiguration SetCommandSchedulerConcurrentLevel(int concurrentLevel)
        {
            ObjectContainer.Resolve<IProcessingMessageScheduler<ProcessingCommand, ICommand, CommandResult>>().SetConcurrencyLevel(concurrentLevel);
            return this;
        }
        /// <summary>Set the concurrent level for application message scheduler.
        /// </summary>
        /// <param name="concurrentLevel"></param>
        /// <returns></returns>
        public ENodeConfiguration SetApplicationMessageSchedulerConcurrentLevel(int concurrentLevel)
        {
            ObjectContainer.Resolve<IProcessingMessageScheduler<ProcessingApplicationMessage, IApplicationMessage, bool>>().SetConcurrencyLevel(concurrentLevel);
            return this;
        }
        /// <summary>Set the concurrent level for domain event scheduler.
        /// </summary>
        /// <param name="concurrentLevel"></param>
        /// <returns></returns>
        public ENodeConfiguration SetDomainEventSchedulerConcurrentLevel(int concurrentLevel)
        {
            ObjectContainer.Resolve<IProcessingMessageScheduler<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>>().SetConcurrencyLevel(concurrentLevel);
            return this;
        }
        /// <summary>Set the concurrent level for exception scheduler.
        /// </summary>
        /// <param name="concurrentLevel"></param>
        /// <returns></returns>
        public ENodeConfiguration SetExceptionSchedulerConcurrentLevel(int concurrentLevel)
        {
            ObjectContainer.Resolve<IProcessingMessageScheduler<ProcessingPublishableExceptionMessage, IPublishableException, bool>>().SetConcurrencyLevel(concurrentLevel);
            return this;
        }

        /// <summary>Initialize all the assembly initializers with the given business assemblies.
        /// </summary>
        /// <returns></returns>
        public ENodeConfiguration InitializeBusinessAssemblies(params Assembly[] assemblies)
        {
            ValidateTypes(assemblies);
            foreach (var assemblyInitializer in _assemblyInitializerServiceTypes.Select(ObjectContainer.Resolve).OfType<IAssemblyInitializer>())
            {
                assemblyInitializer.Initialize(assemblies);
            }
            return this;
        }

        #region Private Methods

        private static void ValidateTypes(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(x => x.IsClass && (typeof(IMessage).IsAssignableFrom(x))))
                {
                    if (!type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(x => x.GetParameters().Count() == 0))
                    {
                        throw new Exception(string.Format("{0} must have a default constructor.", type.FullName));
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

    public static class ConfigurationExtensions
    {
        public static ENodeConfiguration CreateENode(this Configuration configuration, ConfigurationSetting setting = null)
        {
            return ENodeConfiguration.CreateENode(configuration, setting ?? new ConfigurationSetting());
        }
    }
}