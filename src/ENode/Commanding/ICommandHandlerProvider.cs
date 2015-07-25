using System;
using System.Collections.Generic;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a provider to provide the command handlers.
    /// </summary>
    public interface ICommandHandlerProvider
    {
        /// <summary>Get all the handlers for the given command type.
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        IEnumerable<MessageHandlerData<ICommandHandlerProxy>> GetHandlers(Type commandType);
    }
}
