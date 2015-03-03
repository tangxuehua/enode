using System;
using System.Collections.Generic;
using ENode.Commanding;

namespace ENode.Infrastructure
{
    /// <summary>Represents a provider to provide the command handlers.
    /// </summary>
    public interface ICommandHandlerProvider
    {
        /// <summary>Get all the handlers for the given command type.
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        IEnumerable<ICommandHandler> GetHandlers(Type commandType);
    }
}
