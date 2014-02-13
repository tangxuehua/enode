using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    /// <summary>The default implementation of IEventSourcingService.
    /// </summary>
    public class DefaultEventSourcingService : IEventSourcingService, IAssemblyInitializer
    {
        private readonly IAggregateRootInternalHandlerProvider _eventHandlerProvider;
        private readonly IDictionary<Type, Action<IAggregateRoot>> _initializeMethodDict = new Dictionary<Type, Action<IAggregateRoot>>();
        private readonly IDictionary<Type, Action<IAggregateRoot>> _increaseVersionMethodDict = new Dictionary<Type, Action<IAggregateRoot>>();
        private readonly BindingFlags _bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly Type[] _parameterTypes = new Type[] { typeof(IAggregateRoot) };

        public DefaultEventSourcingService(IAggregateRootInternalHandlerProvider eventHandlerProvider)
        {
            _eventHandlerProvider = eventHandlerProvider;
        }

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var aggregateRootType in assembly.GetTypes().Where(ENode.Infrastructure.TypeUtils.IsAggregateRoot))
                {
                    var found = false;
                    var currentType = aggregateRootType;
                    MethodInfo initializeMethod = null;
                    MethodInfo increaseVersionMethod = null;

                    while (!found && currentType != typeof(object))
                    {
                        var entries = from method in currentType.GetMethods(_bindingFlags)
                        let parameters = method.GetParameters()
                        where (method.Name == "Initialize" || method.Name == "IncreaseVersion") && parameters.Length == 0
                        select method;

                        initializeMethod = entries.SingleOrDefault(x => x.Name == "Initialize");
                        increaseVersionMethod = entries.SingleOrDefault(x => x.Name == "IncreaseVersion");

                        if (initializeMethod == null || increaseVersionMethod == null)
                        {
                            currentType = aggregateRootType.BaseType;
                        }
                        else
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        if (initializeMethod == null)
                        {
                            throw new ENodeException("AggregateRoot must has a private parameterless method named 'Initialize'.");
                        }
                        if (increaseVersionMethod == null)
                        {
                            throw new ENodeException("AggregateRoot must has a private parameterless method named 'IncreaseVersion'.");
                        }
                    }

                    RegisterInitializeMethod(aggregateRootType, initializeMethod);
                    RegisterIncreaseVersionMethod(aggregateRootType, increaseVersionMethod);
                }
            }
        }

        public void InitializeAggregateRoot(IAggregateRoot aggregateRoot)
        {
            _initializeMethodDict[aggregateRoot.GetType()](aggregateRoot);
        }
        public void ReplayEvents(IAggregateRoot aggregateRoot, IEnumerable<EventStream> eventStreams)
        {
            foreach (var eventStream in eventStreams)
            {
                VerifyEvent(aggregateRoot, eventStream);
                foreach (var evnt in eventStream.Events)
                {
                    HandleEvent(aggregateRoot, evnt);
                }
                IncreaseAggregateVersion(aggregateRoot);
                if (aggregateRoot.Version != eventStream.Version)
                {
                    throw new ENodeException("Aggregate root version mismatch, aggregateId:{0}, current version:{1}, expected version:{2}", aggregateRoot.UniqueId, aggregateRoot.Version, eventStream.Version);
                }
            }
        }

        private void HandleEvent(IAggregateRoot aggregateRoot, IDomainEvent evnt)
        {
            var handler = _eventHandlerProvider.GetInternalEventHandler(aggregateRoot.GetType(), evnt.GetType());
            if (handler != null)
            {
                handler(aggregateRoot, evnt);
            }
        }
        private void VerifyEvent(IAggregateRoot aggregateRoot, EventStream eventStream)
        {
            if (eventStream.Version > 1 && eventStream.AggregateRootId != aggregateRoot.UniqueId)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate root as the AggregateRootId not matched. EventStream aggregateRootId:{0}, current aggregateRootId:{1}",
                                        eventStream.AggregateRootId,
                                        aggregateRoot.UniqueId);
                throw new ENodeException(errorMessage);
            }

            if (eventStream.Version != aggregateRoot.Version + 1)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate root as the version not matched. EventStream version:{0}, current aggregateRoot version:{1}",
                                        eventStream.Version,
                                        aggregateRoot.Version);
                throw new ENodeException(errorMessage);
            }
        }
        private void IncreaseAggregateVersion(IAggregateRoot aggregateRoot)
        {
            _increaseVersionMethodDict[aggregateRoot.GetType()](aggregateRoot);
        }
        private void RegisterInitializeMethod(Type aggregateRootType, MethodInfo initializeMethod)
        {
            if (_initializeMethodDict.ContainsKey(aggregateRootType))
            {
                throw new ENodeException("Found duplicated 'Initialize' method on aggregate:{0}", aggregateRootType.FullName);
            }
            _initializeMethodDict.Add(aggregateRootType, DelegateFactory.CreateDelegate<Action<IAggregateRoot>>(initializeMethod, _parameterTypes));
        }
        private void RegisterIncreaseVersionMethod(Type aggregateRootType, MethodInfo increaseVersionMethod)
        {
            if (_increaseVersionMethodDict.ContainsKey(aggregateRootType))
            {
                throw new ENodeException("Found duplicated 'IncreaseVersion' method on aggregate:{0}", aggregateRootType.FullName);
            }
            _increaseVersionMethodDict.Add(aggregateRootType, DelegateFactory.CreateDelegate<Action<IAggregateRoot>>(increaseVersionMethod, _parameterTypes));
        }
    }
}
