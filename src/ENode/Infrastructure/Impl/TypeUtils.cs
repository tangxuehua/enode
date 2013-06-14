using System;
using System.ComponentModel;
using System.Linq;
using ENode.Domain;

namespace ENode.Infrastructure
{
    public sealed class TypeUtils
    {
        /// <summary>Convert the given object to a given strong type.
        /// </summary>
        public static T ConvertType<T>(object value)
        {
            if (value == null)
            {
                return default(T);
            }
            TypeConverter typeConverter1 = TypeDescriptor.GetConverter(typeof(T));
            TypeConverter typeConverter2 = TypeDescriptor.GetConverter(value.GetType());
            if (typeConverter1.CanConvertFrom(value.GetType()))
            {
                return (T)typeConverter1.ConvertFrom(value);
            }
            else if (typeConverter2.CanConvertTo(typeof(T)))
            {
                return (T)typeConverter2.ConvertTo(value, typeof(T));
            }
            else
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }
        /// <summary>Check whether a type is a controller.
        /// </summary>
        public static bool IsController(Type type)
        {
            return type != null
                   && type.Name.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase)
                   && !type.IsAbstract
                   && !type.IsInterface;
        }
        /// <summary>Check whether a type is a repository.
        /// </summary>
        public static bool IsRepository(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Repository", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>Check whether a type is a service.
        /// </summary>
        public static bool IsService(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Service", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>Check whether a type is a component.
        /// </summary>
        public static bool IsComponent(Type type)
        {
            return type != null
                 && type.GetCustomAttributes(typeof(ComponentAttribute), false).Count() > 0
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>Check whether a type is an event handler.
        /// </summary>
        public static bool IsEventHandler(Type type)
        {
            return type != null
                 && type.Name.EndsWith("EventHandler", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>Check whether a type is an aggregate root.
        /// </summary>
        public static bool IsAggregateRoot(Type type)
        {
            return type.IsClass
                && !type.IsAbstract
                && typeof(AggregateRoot).IsAssignableFrom(type);
        }
        /// <summary>Check whether a type support serialization.
        /// </summary>
        public static bool IsSerializable(Type type)
        {
            return type != null
                 && type.IsSerializable
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
    }
}