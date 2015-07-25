using System;
using System.Collections.Generic;
using ENode.Infrastructure;

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
        IEnumerable<MessageHandlerData<ICommandAsyncHandlerProxy>> GetHandlers(Type commandType);
    }
}
