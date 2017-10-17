using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECommon.Components;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.EQueue
{
    public static class ENodeExtensions
    {
        public static ENodeConfiguration RegisterTopicProviders(this ENodeConfiguration enodeConfiguration, params Assembly[] assemblies)
        {
            var registeredTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(x => IsTopicProviderType(x)))
                {
                    RegisterComponentType(type);
                    registeredTypes.Add(type);
                }
                foreach (var type in assembly.GetTypes().Where(TypeUtils.IsComponent))
                {
                    if (!registeredTypes.Contains(type))
                    {
                        RegisterComponentType(type);
                    }
                }
            }
            return enodeConfiguration;
        }

        private static bool IsTopicProviderType(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITopicProvider<>));
        }
        private static void RegisterComponentType(Type type)
        {
            var life = ParseComponentLife(type);
            ObjectContainer.RegisterType(type, null, life);
            foreach (var interfaceType in type.GetInterfaces())
            {
                ObjectContainer.RegisterType(interfaceType, type, null, life);
            }
        }
        private static LifeStyle ParseComponentLife(Type type)
        {
            var attributes = type.GetCustomAttributes<ComponentAttribute>(false);
            if (attributes != null && attributes.Any())
            {
                return attributes.First().LifeStyle;
            }
            return LifeStyle.Singleton;
        }
    }
}