using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing;

namespace ENode.Infrastructure.Impl
{
    public class DefaultTypeCodeProvider : ITypeCodeProvider, IAssemblyInitializer
    {
        class TypeCodeMapInfo
        {
            private IDictionary<int, Type> _codeTypeDict = new Dictionary<int, Type>();
            private IDictionary<Type, int> _typeCodeDict = new Dictionary<Type, int>();

            public bool IsTypeCodeExist(Type type)
            {
                return _typeCodeDict.ContainsKey(type);
            }
            public int GetTypeCode(Type type)
            {
                if (!_typeCodeDict.ContainsKey(type))
                {
                    throw new Exception(string.Format("Code for type:{0} not exist.", type.FullName));
                }
                return _typeCodeDict[type];
            }
            public Type GetType(int typeCode)
            {
                if (!_codeTypeDict.ContainsKey(typeCode))
                {
                    throw new Exception(string.Format("Type for code:{0} not exist.", typeCode));
                }
                return _codeTypeDict[typeCode];
            }

            public void RegisterType<T>(int code)
            {
                RegisterType(code, typeof(T));
            }
            public void RegisterType(int code, Type type)
            {
                if (_codeTypeDict.ContainsKey(code))
                {
                    throw new Exception(
                        string.Format("Code type already exist, cannot register again. code: {0}, existing type: {1}, current register type: {2}",
                        code, _codeTypeDict[code], type));
                }
                if (_typeCodeDict.ContainsKey(type))
                {
                    throw new Exception(
                        string.Format("Type code already exist, cannot register again. type: {0}, existing code: {1}, current register code: {2}",
                        type, _typeCodeDict[type], code));
                }
                _codeTypeDict.Add(code, type);
                _typeCodeDict.Add(type, code);
            }
        }

        private readonly TypeCodeMapInfo _aggregateRootTypeCodeMapInfo = new TypeCodeMapInfo();
        private readonly TypeCodeMapInfo _commandTypeCodeMapInfo = new TypeCodeMapInfo();
        private readonly TypeCodeMapInfo _domainEventTypeCodeMapInfo = new TypeCodeMapInfo();
        private readonly TypeCodeMapInfo _applicationMessageTypeCodeMapInfo = new TypeCodeMapInfo();
        private readonly TypeCodeMapInfo _publishableExceptionTypeCodeMapInfo = new TypeCodeMapInfo();
        private readonly TypeCodeMapInfo _messageHandlerTypeCodeMapInfo = new TypeCodeMapInfo();

        public int GetTypeCode(Type type)
        {
            if (typeof(IAggregateRoot).IsAssignableFrom(type))
            {
                return _aggregateRootTypeCodeMapInfo.GetTypeCode(type);
            }
            else if (typeof(ICommand).IsAssignableFrom(type))
            {
                return _commandTypeCodeMapInfo.GetTypeCode(type);
            }
            else if (typeof(IDomainEvent).IsAssignableFrom(type))
            {
                return _domainEventTypeCodeMapInfo.GetTypeCode(type);
            }
            else if (typeof(IApplicationMessage).IsAssignableFrom(type))
            {
                return _applicationMessageTypeCodeMapInfo.GetTypeCode(type);
            }
            else if (typeof(IPublishableException).IsAssignableFrom(type))
            {
                return _publishableExceptionTypeCodeMapInfo.GetTypeCode(type);
            }
            else if (typeof(IMessageHandler).IsAssignableFrom(type))
            {
                return _messageHandlerTypeCodeMapInfo.GetTypeCode(type);
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid type '{0}', not allowed to get code by type.", type.FullName));
            }
        }
        public Type GetType<T>(int typeCode)
        {
            var type = typeof(T);

            if (type == typeof(IAggregateRoot))
            {
                return _aggregateRootTypeCodeMapInfo.GetType(typeCode);
            }
            else if (type == typeof(ICommand))
            {
                return _commandTypeCodeMapInfo.GetType(typeCode);
            }
            else if (type == typeof(IDomainEvent))
            {
                return _domainEventTypeCodeMapInfo.GetType(typeCode);
            }
            else if (type == typeof(IApplicationMessage))
            {
                return _applicationMessageTypeCodeMapInfo.GetType(typeCode);
            }
            else if (type == typeof(IPublishableException))
            {
                return _publishableExceptionTypeCodeMapInfo.GetType(typeCode);
            }
            else if (type == typeof(IMessageHandler))
            {
                return _messageHandlerTypeCodeMapInfo.GetType(typeCode);
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid type '{0}', not allowed to get type by code.", type.FullName));
            }
        }
        public void RegisterType<T>(int code)
        {
            RegisterType(code, typeof(T));
        }
        public void RegisterType(int code, Type type)
        {
            if (typeof(IAggregateRoot).IsAssignableFrom(type))
            {
                _aggregateRootTypeCodeMapInfo.RegisterType(code, type);
            }
            else if (typeof(ICommand).IsAssignableFrom(type))
            {
                _commandTypeCodeMapInfo.RegisterType(code, type);
            }
            else if (typeof(IDomainEvent).IsAssignableFrom(type))
            {
                _domainEventTypeCodeMapInfo.RegisterType(code, type);
            }
            else if (typeof(IApplicationMessage).IsAssignableFrom(type))
            {
                _applicationMessageTypeCodeMapInfo.RegisterType(code, type);
            }
            else if (typeof(IPublishableException).IsAssignableFrom(type))
            {
                _publishableExceptionTypeCodeMapInfo.RegisterType(code, type);
            }
            else if (typeof(IMessageHandler).IsAssignableFrom(type))
            {
                _messageHandlerTypeCodeMapInfo.RegisterType(code, type);
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid type '{0}', not allowed to register type code.", type.FullName));
            }
        }

        public void Initialize(params Assembly[] assemblies)
        {
            foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && (
                    typeof(IAggregateRoot).IsAssignableFrom(x) ||
                    typeof(ICommand).IsAssignableFrom(x) ||
                    typeof(IDomainEvent).IsAssignableFrom(x) ||
                    typeof(IApplicationMessage).IsAssignableFrom(x) ||
                    typeof(IPublishableException).IsAssignableFrom(x) ||
                    typeof(IMessageHandler).IsAssignableFrom(x)))))
            {
                var codeAttribute = type.GetCustomAttributes<CodeAttribute>(false).SingleOrDefault();
                if (codeAttribute != null)
                {
                    RegisterType(codeAttribute.Code, type);
                }
                else if (!IsTypeCodeExist(type))
                {
                    throw new Exception(string.Format("Code for type: {0} not exist.", type.FullName));
                }
            }
        }

        private bool IsTypeCodeExist(Type type)
        {
            if (typeof(IAggregateRoot).IsAssignableFrom(type))
            {
                return _aggregateRootTypeCodeMapInfo.IsTypeCodeExist(type);
            }
            else if (typeof(ICommand).IsAssignableFrom(type))
            {
                return _commandTypeCodeMapInfo.IsTypeCodeExist(type);
            }
            else if (typeof(IDomainEvent).IsAssignableFrom(type))
            {
                return _domainEventTypeCodeMapInfo.IsTypeCodeExist(type);
            }
            else if (typeof(IApplicationMessage).IsAssignableFrom(type))
            {
                return _applicationMessageTypeCodeMapInfo.IsTypeCodeExist(type);
            }
            else if (typeof(IPublishableException).IsAssignableFrom(type))
            {
                return _publishableExceptionTypeCodeMapInfo.IsTypeCodeExist(type);
            }
            else if (typeof(IMessageHandler).IsAssignableFrom(type))
            {
                return _messageHandlerTypeCodeMapInfo.IsTypeCodeExist(type);
            }
            return false;
        }
    }
}
