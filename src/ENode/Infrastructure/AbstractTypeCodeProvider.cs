using System;
using System.Collections.Generic;

namespace ENode.Infrastructure
{
    public abstract class AbstractTypeCodeProvider<TType> : ITypeCodeProvider<TType>
    {
        private IDictionary<int, Type> _codeTypeDict = new Dictionary<int, Type>();
        private IDictionary<Type, int> _typeCodeDict = new Dictionary<Type, int>();

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

        protected void RegisterType<T>(int code)
        {
            RegisterType(code, typeof(T));
        }
        protected void RegisterType(int code, Type type)
        {
            _codeTypeDict.Add(code, type);
            _typeCodeDict.Add(type, code);
        }
    }
}
