using System;
using ENode.Infrastructure.Impl;

namespace ENode.Infrastructure
{
    /// <summary>Represents a provider to provide the type and type name mapping information.
    /// </summary>
    public interface ITypeNameProvider
    {
        /// <summary>Get the type name of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetTypeName(Type type);
        /// <summary>Get the type of the given type name.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        Type GetType(string typeName);
    }
}
