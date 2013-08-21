using System;

namespace ENode.Infrastructure.Retring
{
    /// <summary>Represents a retry service interface.
    /// </summary>
    public interface IRetryService
    {
        /// <summary>Initialize the retry service.
        /// </summary>
        /// <param name="period"></param>
        void Initialize(long period);
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute success within the max retry count, returns true; otherwise, returns false;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        bool TryAction(string actionName, Func<bool> action, int maxRetryCount);
        /// <summary>Put the action into retry queue, and retry to execute the action with some period until the action execute success.
        /// </summary>
        /// <param name="actionInfo"></param>
        void RetryInQueue(ActionInfo actionInfo);
    }
}
