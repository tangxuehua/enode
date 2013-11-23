using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandCompletionEventManager.
    /// </summary>
    public class DefaultCommandCompletionEventManager : ICommandCompletionEventManager, IAssemblyInitializer
    {
        private readonly IList<Type> _completionEventTypes = new List<Type>();

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAllCompletionEventsInAssembly(assembly);
            }
        }
        public bool IsCompletionEvent(IDomainEvent domainEvent)
        {
            return _completionEventTypes.Contains(domainEvent.GetType());
        }

        private void RegisterAllCompletionEventsInAssembly(Assembly assembly)
        {
            foreach (var completionEventType in assembly.GetTypes().Where(IsCompletionEventType))
            {
                if (_completionEventTypes.Contains(completionEventType))
                {
                    throw new Exception(string.Format("Duplicated completion event type:{0}", completionEventType));
                }
                _completionEventTypes.Add(completionEventType);
            }
        }
        private bool IsCompletionEventType(Type type)
        {
            return type.IsInterface == false && type.IsAbstract == false && typeof(IDomainEvent).IsAssignableFrom(type) && typeof(ICompletionEvent).IsAssignableFrom(type);
        }
    }
}
