using System;
using Autofac;
using ENode.Infrastructure;

namespace ENode.Autofac
{
    /// <summary>
    /// 
    /// </summary>
    public class AutofacObjectContainer : IObjectContainer
    {
        private readonly IContainer _container;

        /// <summary>
        /// 
        /// </summary>
        public AutofacObjectContainer()
        {
            _container = new ContainerBuilder().Build();
        }
        /// <summary>
        /// 
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="implementationType"></param>
        /// <param name="life"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="life"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementer"></typeparam>
        /// <param name="life"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementer"></typeparam>
        /// <param name="instance"></param>
        public void RegisterInstance<TService, TImplementer>(TImplementer instance)
            where TService : class
            where TImplementer : class, TService
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance).As<TService>().SingleInstance();
            builder.Update(_container);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService Resolve<TService>() where TService : class
        {
            return _container.Resolve<TService>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}

