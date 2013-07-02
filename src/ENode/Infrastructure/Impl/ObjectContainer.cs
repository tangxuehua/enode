using System;
using System.Reflection;

namespace ENode.Infrastructure
{
    /// <summary>Ioc对象容器全局静态访问类
    /// </summary>
    public class ObjectContainer
    {
        private static IObjectContainer _container;

        /// <summary>Set the object container for the framework.
        /// </summary>
        /// <param name="container"></param>
        public static void SetCurrentContainer(IObjectContainer container)
        {
            _container = container;
        }

        /// <summary>Get the instance of the current object container.
        /// </summary>
        public static IObjectContainer Current { get { return _container; } }

        /// <summary>Register a type and all its interfaces.
        /// </summary>
        /// <param name="type"></param>
        public static void RegisterType(Type type)
        {
            _container.RegisterType(type);
        }
        /// <summary>Register a type and all its interfaces with a specific key.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        public static void RegisterType(Type type, string key)
        {
            _container.RegisterType(type, key);
        }
        /// <summary>Scan the given assemblies and register all the types which match the predicate condition.
        /// </summary>
        /// <param name="typePredicate"></param>
        /// <param name="assemblies"></param>
        public static void RegisterTypes(Func<Type, bool> typePredicate, params Assembly[] assemblies)
        {
            _container.RegisterTypes(typePredicate, assemblies);
        }
        /// <summary>Register the service type's default implementation type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="life"></param>
        public static void Register<TService, TImpl>(LifeStyle life = LifeStyle.Singleton) where TService : class where TImpl : class, TService
        {
            _container.Register<TService, TImpl>(life);
        }
        /// <summary>Register the service type's default implementation type and specified a specific key.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="key"></param>
        /// <param name="life"></param>
        public static void Register<TService, TImpl>(string key, LifeStyle life = LifeStyle.Singleton) where TService : class where TImpl : class, TService
        {
            _container.Register<TService, TImpl>(key, life);
        }
        /// <summary>Register the service type's default implementation instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="life"></param>
        public static void Register<T>(T instance, LifeStyle life = LifeStyle.Singleton) where T : class
        {
            _container.Register<T>(instance, life);
        }
        /// <summary>Register the service type's default implementation instance and specified a specific key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        /// <param name="life"></param>
        public static void Register<T>(T instance, string key, LifeStyle life = LifeStyle.Singleton) where T : class
        {
            _container.Register<T>(instance, key, life);
        }
        /// <summary>Check whether a type is registered in the container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRegistered(Type type)
        {
            return _container.IsRegistered(type);
        }
        /// <summary>Check whether a type with the given key is registered in the container.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsRegistered(Type type, string key)
        {
            return _container.IsRegistered(type, key);
        }
        /// <summary>Resolve a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }
        /// <summary>Resolve a type with the given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Resolve<T>(string key) where T : class
        {
            return _container.Resolve<T>(key);
        }
        /// <summary>Resolve a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Resolve(Type type)
        {
            return _container.Resolve(type);
        }
        /// <summary>Resolve a type with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Resolve(string key, Type type)
        {
            return _container.Resolve(key, type);
        }
    }
}
