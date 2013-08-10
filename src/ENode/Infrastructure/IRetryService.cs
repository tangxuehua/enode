using System;

namespace ENode.Infrastructure {
    /// <summary>Represents a retry service interface.
    /// </summary>
    public interface IRetryService {
        void Initialize(long period);
        bool TryAction(string actionName, Func<bool> action, int maxRetryCount);
        void RetryInQueue(ActionInfo actionInfo);
    }
}
