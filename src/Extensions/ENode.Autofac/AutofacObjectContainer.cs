using System;
using Autofac;
using ENode.Infrastructure;

namespace ENode.Autofac
{
    /// <summary>Autofac implementation of IObjectContainer.
    /// </summary>
    public class AutofacObjectContainer : IObjectContainer
    {
        private readonly IContainer _container;

        /// <summary>Default constructor.
        /// </summary>
        public AutofacObjectContainer()
        {
            _container = new ContainerBuilder().Build();
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="containerBuilder"></param>
        public AutofacObjectContainer(ContainerBuilder containerBuilder)
        {
            _container = containerBuilder.Build();
        }

        /// <summary>Represents the inner autofac container.
        /// </summary>
        public IContainer Container
        {
            get
            {
                return _container;
            }
        }
        /// <summary>Register a implementation type.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void RegisterType(Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType(implementationType);
            if (life == LifeStyle.Singleton)
            {
                registrationBuilder.SingleInstance();
            }
            builder.Update(_container);
        }
        /// <summary>Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void RegisterType(Type serviceType, Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType(implementationType).As(serviceType);
            if (life == LifeStyle.Singleton)
            {
                registrationBuilder.SingleInstance();
            }
            builder.Update(_container);
        }
        /// <summary>Register a implementer type as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void Register<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton)
            where TService : class
            where TImplementer : class, TService
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType<TImplementer>().As<TService>();
            if (life == LifeStyle.Singleton)
            {
                registrationBuilder.SingleInstance();
            }
            builder.Update(_container);
        }
        /// <summary>Register a implementer type instance as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="instance">The implementer type instance.</param>
        public void RegisterInstance<TService, TImplementer>(TImplementer instance)
            where TService : class
            where TImplementer : class, TService
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance).As<TService>().SingleInstance();
            builder.Update(_container);
        }
        /// <summary>Resolve a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        public TService Resolve<TService>() where TService : class
        {
            return _container.Resolve<TService>();
        }
        /// <summary>Resolve a service.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The component instance that provides the service.</returns>
        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}

