using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents an object container.
    /// </summary>
    public class ObjectContainer
    {
        private static IObjectContainer _container;

        /// <summary>Represents the current object container.
        /// </summary>
        public static IObjectContainer Current
        {
            get
            {
                return _container;
            }
        }

        /// <summary>Set the object container.
        /// </summary>
        /// <param name="container"></param>
        public static void SetContainer(IObjectContainer container)
        {
            _container = container;
        }

        /// <summary>Register a implementation type.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public static void RegisterType(Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            _container.RegisterType(implementationType, life);
        }
        /// <summary>Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The implementation type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public static void RegisterType(Type serviceType, Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            _container.RegisterType(serviceType, implementationType, life);
        }
        /// <summary>Register a implementer type as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="life">The life cycle of the implementer type.</param>
        public static void Register<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton)
            where TService : class
            where TImplementer : class, TService
        {
            _container.Register<TService, TImplementer>(life);
        }
        /// <summary>Register a implementer type instance as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="instance">The implementer type instance.</param>
        public static void RegisterInstance<TService, TImplementer>(TImplementer instance)
            where TService : class
            where TImplementer : class, TService
        {
            _container.RegisterInstance<TService, TImplementer>(instance);
        }
        /// <summary>Resolve a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        public static TService Resolve<TService>() where TService : class
        {
            return _container.Resolve<TService>();
        }
        /// <summary>Resolve a service.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The component instance that provides the service.</returns>
        public static object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}
