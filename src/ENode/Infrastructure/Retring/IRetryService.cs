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
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        void TryAction(string actionName, Action action, int maxRetryCount, Action nextAction);
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        void TryAction(string actionName, Func<bool> action, int maxRetryCount, Action nextAction);
        /// <summary>Try to execute the given action with the given max retry count.
        /// <remarks>If the action execute still failed within the max retry count, then put the action into the retry queue;</remarks>
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <param name="nextAction"></param>
        void TryAction(string actionName, Func<bool> action, int maxRetryCount, Func<bool> nextAction);
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
