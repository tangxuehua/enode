using System;
using System.Linq;
using ECommon.Components;
using ENode.Domain;

namespace ENode.Infrastructure
{
    /// <summary>A utility class provides type related methods.
    /// </summary>
    public class TypeUtils
    {
        /// <summary>Check whether a type is a component type.
        /// </summary>
        public static bool IsComponent(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetCustomAttributes(typeof(ComponentAttribute), false).Any();
        }
        /// <summary>Check whether a type is an aggregate root type.
        /// </summary>
        public static bool IsAggregateRoot(Type type)
        {
            return type.IsClass && !type.IsAbstract && typeof(IAggregateRoot).IsAssignableFrom(type);
        }
    }
}