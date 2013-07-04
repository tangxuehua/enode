using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac.Builder;
using ENode.Infrastructure;
using Autofac;

namespace ENode.Extensions.Container.Autofac
{
    public class AutofacObjectContainer : IObjectContainer
    {
        private readonly IContainer _container;

        public AutofacObjectContainer()
        {
            _container = new ContainerBuilder().Build();
        }

        public void RegisterType(Type type)
        {
            var builder = new ContainerBuilder();

            var life = ParseLife(type);

            if (!IsRegistered(type))
                builder.RegisterType(type).Life(life);

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (!IsRegistered(interfaceType))
                    builder.RegisterType(type).As(interfaceType).Life(life);
            }

            builder.Update(_container);
        }

        public void RegisterType(Type type, string key)
        {
            var builder = new ContainerBuilder();

            var life = ParseLife(type);

            if (!IsRegistered(type, key))
                builder.RegisterType(type).Named(key, type).Life(life);

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (!IsRegistered(interfaceType, key))
                    builder.RegisterType(type).Named(key, interfaceType).Life(life);
            }

            builder.Update(_container);
        }
        public void RegisterTypes(Func<Type, bool> typePredicate, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes().Where(x => typePredicate(x)))
                    RegisterType(type);
            }
        }
        public void Register<TService, TImpl>(LifeStyle life) where TService : class where TImpl : class, TService
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TImpl>().As<TService>().Life(life);
            builder.Update(_container);
        }
        public void Register<TService, TImpl>(string key, LifeStyle life) where TService : class where TImpl : class, TService
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TImpl>().As<TService>().Life(life);
            builder.Update(_container);
        }
        public void Register<T>(T instance, LifeStyle life) where T : class
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance);
            builder.Update(_container);
        }
        public void Register<T>(T instance, string key, LifeStyle life) where T : class
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance).Named<T>(key);
            builder.Update(_container);
        }
        public bool IsRegistered(Type type)
        {
            return  _container.IsRegistered(type);
        }
        public bool IsRegistered(Type type, string key)
        {
            return _container.IsRegisteredWithName(key, type);
        }
        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }
        public T Resolve<T>(string key) where T : class
        {
            return _container.ResolveNamed<T>(key);
        }
        public object Resolve(Type type)
        {
            if (!IsRegistered(type))
            {
                var builder = new ContainerBuilder();
                builder.RegisterType(type);
                builder.Update(_container);
            }    
            
            return _container.Resolve(type);
        }
        public object Resolve(string key, Type type)
        {
            return _container.ResolveNamed(key, type);
        }

        private LifeStyle ParseLife(Type type)
        {
            var componentAttributes = type.GetCustomAttributes(typeof(ComponentAttribute), false);
            return componentAttributes.Count() <= 0 ? LifeStyle.Transient : (componentAttributes[0] as ComponentAttribute).LifeStyle;
        }
    }

    public static class AutofacIocExtensions
    {
        public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> Life<T>(this 
            IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration, LifeStyle life)
        {
            if (life == LifeStyle.Singleton)
            {
                return registration.SingleInstance();
            }
            return registration;
        }

        public static Configuration UseAutofacObjectContainer(this Configuration configuration)
        {
            ObjectContainer.SetCurrentContainer(new AutofacObjectContainer());
            return configuration;
        }
    }
}
