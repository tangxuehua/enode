using System;
using System.Reflection;

namespace ENode.Infrastructure
{
    /// <summary>Represents a ioc object container.
    /// </summary>
    public interface IObjectContainer
    {
        /// <summary>Register a type and all its interfaces.
        /// </summary>
        /// <param name="type"></param>
        void RegisterType(Type type);
        /// <summary>Register a type and all its interfaces with a specific key.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        void RegisterType(Type type, string key);
        /// <summary>Scan the given assemblies and register all the types which match the predicate condition.
        /// </summary>
        /// <param name="typePredicate"></param>
        /// <param name="assemblies"></param>
        void RegisterTypes(Func<Type, bool> typePredicate, params Assembly[] assemblies);
        /// <summary>Register the service type's default implementation type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="life"></param>
        void Register<TService, TImpl>(LifeStyle life) where TService : class where TImpl : class, TService;
        /// <summary>Register the service type's default implementation type and specified a specific key.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="key"></param>
        /// <param name="life"></param>
        void Register<TService, TImpl>(string key, LifeStyle life) where TService : class where TImpl : class, TService;
        /// <summary>Register the service type's default implementation instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="life"></param>
        void Register<T>(T instance, LifeStyle life) where T : class;
        /// <summary>Register the service type's default implementation instance and specified a specific key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        /// <param name="life"></param>
        void Register<T>(T instance, string key, LifeStyle life) where T : class;
        /// <summary>Check whether a type is registered in the container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegistered(Type type);
        /// <summary>Check whether a type with the given key is registered in the container.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsRegistered(Type type, string key);
        /// <summary>Resolve a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : class;
        /// <summary>Resolve a type with the given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Resolve<T>(string key) where T : class;
        /// <summary>Resolve a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(Type type);
        /// <summary>Resolve a type with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(string key, Type type);
    }
}
