using System;

namespace ENode.Infrastructure.Retring
{
    /// <summary>Represents a retry service interface.
    /// </summary>
    public interface IRetryService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="period"></param>
        void Initialize(long period);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        bool TryAction(string actionName, Func<bool> action, int maxRetryCount);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionInfo"></param>
        void RetryInQueue(ActionInfo actionInfo);
    }
}
