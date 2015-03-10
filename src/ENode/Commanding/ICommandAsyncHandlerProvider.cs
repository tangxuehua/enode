using System;
using System.Collections.Generic;

namespace ENode.Commanding
{
    /// <summary>Represents a provider to provide the aggregate command handlers.
    /// </summary>
    public interface ICommandAsyncHandlerProvider
    {
        /// <summary>Get all the async handlers for the given command type.
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        IEnumerable<ICommandAsyncHandlerProxy> GetHandlers(Type commandType);
    }
}
