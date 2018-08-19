using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateRepositoryProvider : IAggregateRepositoryProvider, IAssemblyInitializer
    {
        private readonly IDictionary<Type, IAggregateRepositoryProxy> _repositoryDict = new Dictionary<Type, IAggregateRepositoryProxy>();

        public IAggregateRepositoryProxy GetRepository(Type aggregateRootType)
        {
            if (_repositoryDict.TryGetValue(aggregateRootType, out IAggregateRepositoryProxy proxy))
            {
                return proxy;
            }
            return null;
        }
        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var aggregateRepositoryType in assemblies.SelectMany(assembly => assembly.GetTypes().Where(IsAggregateRepositoryType)))
            {
                RegisterAggregateRepository(aggregateRepositoryType);
            }
        }

        private void RegisterAggregateRepository(Type aggregateRepositoryType)
        {
            var repositoryInterfaceTypes = ScanAggregateRepositoryInterfaces(aggregateRepositoryType);

            foreach (var repositoryInterfaceType in repositoryInterfaceTypes)
            {
                var aggregateType = repositoryInterfaceType.GetGenericArguments().Single();
                var proxyType = typeof(AggregateRepositoryProxy<>).MakeGenericType(aggregateType);
                var aggregateRepositoryProxy = Activator.CreateInstance(proxyType, new[] { ObjectContainer.Resolve(aggregateRepositoryType) }) as IAggregateRepositoryProxy;
                _repositoryDict.Add(aggregateType, aggregateRepositoryProxy);
            }
        }
        private bool IsAggregateRepositoryType(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && ScanAggregateRepositoryInterfaces(type).Any();
        }
        private IEnumerable<Type> ScanAggregateRepositoryInterfaces(Type type)
        {
            return type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAggregateRepository<>));
        }
    }
}
