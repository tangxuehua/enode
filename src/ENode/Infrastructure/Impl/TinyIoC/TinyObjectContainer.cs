using System;
using System.Linq;
using System.Reflection;
using ENode.Infrastructure;
using TinyIoC;

namespace ENode.Infrastructure
{
    public class TinyObjectContainer : IObjectContainer
    {
        private TinyIoCContainer _container;

        public TinyObjectContainer()
        {
            _container = TinyIoCContainer.Current;
        }

        public void RegisterType(Type type)
        {
            var life = ParseLife(type);

            if (!IsRegistered(type))
            {
                _container.Register(type).Life(life);
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (!IsRegistered(interfaceType))
                {
                    _container.Register(interfaceType, type).Life(life);
                }
            }
        }
        public void RegisterType(Type type, string key)
        {
            var life = ParseLife(type);

            if (!IsRegistered(type, key))
            {
                _container.Register(type, key).Life(life);
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (!IsRegistered(interfaceType, key))
                {
                    _container.Register(interfaceType, type, key).Life(life);
                }
            }
        }
        public void RegisterTypes(Func<Type, bool> typePredicate, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes().Where(x => typePredicate(x)))
                {
                    RegisterType(type);
                }
            }
        }
        public void Register<TService, TImpl>(LifeStyle life) where TService : class where TImpl : class, TService
        {
            _container.Register<TService, TImpl>().Life(life);
        }
        public void Register<TService, TImpl>(string key, LifeStyle life) where TService : class where TImpl : class, TService
        {
            _container.Register<TService, TImpl>(key).Life(life);
        }
        public void Register<T>(T instance, LifeStyle life) where T : class
        {
            _container.Register<T>(instance).Life(life);
        }
        public void Register<T>(T instance, string key, LifeStyle life) where T : class
        {
            _container.Register<T>(instance, key).Life(life);
        }
        public bool IsRegistered(Type type)
        {
            return _container.CanResolve(type);
        }
        public bool IsRegistered(Type type, string key)
        {
            return _container.CanResolve(type, key, ResolveOptions.Default);
        }
        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }
        public T Resolve<T>(string key) where T : class
        {
            return _container.Resolve<T>(key);
        }
        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }
        public object Resolve(string key, Type type)
        {
            return _container.Resolve(type, key, ResolveOptions.Default);
        }

        private LifeStyle ParseLife(Type type)
        {
            var componentAttributes = type.GetCustomAttributes(typeof(ComponentAttribute), false);
            return componentAttributes.Count() <= 0 ? LifeStyle.Transient : (componentAttributes[0] as ComponentAttribute).LifeStyle;
        }
    }

    public static class TinyIoCContainerExtensions
    {
        public static TinyIoCContainer.RegisterOptions Life(this TinyIoCContainer.RegisterOptions registration, LifeStyle life)
        {
            if (life == LifeStyle.Singleton)
            {
                return registration.AsSingleton();
            }
            return registration.AsMultiInstance();
        }
    }
}

