using System;

namespace ENode.Infrastructure.Retring
{
    /// <summary>Defines a service interface to execute action.
    /// </summary>
    public interface IActionExecutionService
    {
        /// <summary>Initialize the retry service.
        /// </summary>
        /// <param name="period"></param>
        void Initialize(int period);
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        void TryAction(string actionName, Func<bool> action, int maxRetryCount, ActionInfo nextAction);
        /// <summary>Try to execute the given action with the given max retry count. If success then returns true; otherwise, returns false.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        bool TryRecursively(string actionName, Func<bool> action, int maxRetryCount);
    }
}
