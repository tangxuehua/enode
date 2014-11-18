using System;
using System.Collections.Generic;

namespace ENode.Infrastructure
{
    /// <summary>Represents a provider to provide the handlers.
    /// </summary>
    public interface IHandlerProvider<THandlerInterface> where THandlerInterface : class
    {
        /// <summary>Get all the handlers for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<THandlerInterface> GetHandlers(Type type);
        /// <summary>Check whether a given type is a handler type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsHandler(Type type);
    }
}
