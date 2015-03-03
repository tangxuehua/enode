using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a provider to provide the type and code mapping information.
    /// </summary>
    public interface ITypeCodeProvider
    {
        /// <summary>Get the code of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        int GetTypeCode(Type type);
        /// <summary>Get the type of the given type code.
        /// </summary>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        Type GetType(int typeCode);
    }
}
