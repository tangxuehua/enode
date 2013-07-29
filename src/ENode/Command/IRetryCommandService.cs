using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents a service to retry a command.
    /// </summary>
    public interface IRetryCommandService
    {
        /// <summary>Retry the given command.
        /// </summary>
        void RetryCommand(CommandInfo commandInfo, ErrorInfo errorInfo, ActionInfo retrySuccessCallbackAction);
    }
}
